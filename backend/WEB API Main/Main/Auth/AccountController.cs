using BusinessSpecificLogic;
using Devcorner.NIdenticon;
using Devcorner.NIdenticon.BrushGenerators;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Reusable;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;

namespace ReusableWebAPI.Auth
{
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private AuthRepository _repo = null;
        private IRepository<User> _userRepository;

        public AccountController()
        {
            _repo = new AuthRepository();
        }

        // POST api/Account/Register
        [AllowAnonymous]
        [HttpPost, Route("Register")]
        public async Task<IHttpActionResult> Register([FromBody] string s)
        {
            UserModel userModel;
            try
            {
                userModel = JsonConvert.DeserializeObject<UserModel>(s);
            }
            catch (Exception e)
            {
                return InternalServerError(new Exception(e.Message));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            IdentityResult result = await _repo.RegisterUser(userModel);

            IHttpActionResult errorResult = GetErrorResult(result);

            if (errorResult != null)
            {
                return errorResult;
            }


            Bitmap identicon;
            try
            {
                var bg = new StaticColorBrushGenerator(StaticColorBrushGenerator.ColorFromText(userModel.UserName));
                identicon = new IdenticonGenerator("MD5")
                    .WithSize(100, 100)
                    .WithBackgroundColor(System.Drawing.Color.White)
                    .WithBlocks(4, 4)
                    .WithBlockGenerators(IdenticonGenerator.ExtendedBlockGeneratorsConfig)
                    .WithBrushGenerator(bg)
                    .Create(userModel.UserName);
            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception(ex.Message));
            }

            User theNewUser = new User();
            theNewUser.UserName = userModel.UserName;
            theNewUser.Value = userModel.UserName;


            ImageConverter converter = new ImageConverter();

            try
            {
                theNewUser.Identicon64 = Convert.ToBase64String(ConvertBitMapToByteArray(identicon));
                theNewUser.Identicon = (byte[])converter.ConvertTo(identicon, typeof(byte[]));

            }
            catch (Exception ex)
            {
                return InternalServerError(new Exception(ex.Message));
            }


            /*Begin Transaction*/
            try
            {
                using (var context = new MainContext())
                {
                    _userRepository = new Repository<User>(context);
                    _userRepository.Add(theNewUser);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }

            return Ok();
        }

        private byte[] ConvertBitMapToByteArray(Bitmap bitmap)
        {
            byte[] result = null;

            if (bitmap != null)
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Bmp);
                    result = stream.ToArray();
                }
            }

            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }

            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}