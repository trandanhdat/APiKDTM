//using APi.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authentication.BearerToken;
//using System.IdentityModel.Tokens.Jwt;
//using APi.Services;
//using Microsoft.Extensions.Options;
//using System.Text;
//using Microsoft.IdentityModel.Tokens;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.AspNetCore.Mvc.ApiExplorer;
//using Microsoft.EntityFrameworkCore;

//namespace APi.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class TokenController : ControllerBase
//    {
//        private readonly DbLoaiContext _dbLoaiContext;
//        private readonly ILogger<TokenController> _logger;
//        private readonly AppSettings _appSettings;
//        public TokenController(DbLoaiContext dbLoaiContext,IOptions<AppSettings> option,ILogger<TokenController> logger)
//        {
//            _dbLoaiContext = dbLoaiContext;
//            _logger = logger;
//            _appSettings = option.Value;
//        }
//        [Route("/Login")]
//        [HttpPost]
//        public IActionResult Login(LoginRequestDto userkt)
//        {
//            var user = _dbLoaiContext.userModels.SingleOrDefault(p => p.user == userkt.User &&
//            p.password == userkt.Password);
//            if (user == null)
//            {
//                return Ok(new
//                {
//                    success = false,
//                    message = "khong ton tai user",
//                    Data = ""

//                });
//            }
//            else
//            {
//                return Ok(new
//                {
//                    Success = true,

//                    Message = "Xac thuc thanh cong",
//                    Data = GenerateToken(user)

//                });
//            }
//        }
//        private TokenModel GenerateToken(UserModel nguoidung)
//        {
//            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
//            var SecretByte = Encoding.UTF8.GetBytes(_appSettings.SecretKey);

//            var tokenDricription = new SecurityTokenDescriptor
//            {
//                Subject = new ClaimsIdentity(new[]
//                {
//                    new Claim(JwtRegisteredClaimNames.Email,nguoidung.email),
//                    new Claim(JwtRegisteredClaimNames.Sub,nguoidung.email),
//                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),

//                    new Claim("UserName",nguoidung.user),
//                    new Claim("id",nguoidung.id.ToString()),
//                }) ,
//                Expires = DateTime.UtcNow.AddMinutes(1),
//                SigningCredentials  = new SigningCredentials(new SymmetricSecurityKey(SecretByte),
//                SecurityAlgorithms.HmacSha512Signature)
//            };
//            var token = handler.CreateToken(tokenDricription);
//            var accessToken = handler.WriteToken(token);
//            var RefreshToke = GenerateRefreshToken();

//            var refreshTokenEntity = _dbLoaiContext.refreshTokens
//                .FirstOrDefault(r => r.UserId == nguoidung.id);

//            if (refreshTokenEntity == null)
//            {
//                 Nếu chưa có thì tạo mới
//                refreshTokenEntity = new RefreshToken
//                {
//                    Id = Guid.NewGuid(),
//                    UserId = nguoidung.id
//                };
//                _dbLoaiContext.refreshTokens.Add(refreshTokenEntity);
//            }

//             Cập nhật lại token cũ
//            refreshTokenEntity.JwtId = token.Id;
//            refreshTokenEntity.Token = RefreshToke;
//            refreshTokenEntity.IsUsed = false;
//            refreshTokenEntity.IsRevoked = false;
//            refreshTokenEntity.IssuedAt = DateTime.UtcNow;
//            refreshTokenEntity.ExpiredAt = DateTime.UtcNow.AddSeconds(20);

//            _dbLoaiContext.SaveChanges();
//            return new TokenModel
//            {
//                AccessToken = accessToken,
//                RefreshToken = RefreshToke
//            };
//        }
//        private string GenerateRefreshToken()
//        {
//            var random = new byte[32];
//            using (var rng = RandomNumberGenerator.Create())
//            {
//                rng.GetBytes(random);
//            }
//            return Convert.ToBase64String(random);
//        }
//        [HttpPost("renewToken")]
//        public async Task<IActionResult> RenewToken(TokenModel model)
//        {
//            var jwtTokenHandler = new JwtSecurityTokenHandler();
            
//            var secretKeyBytes = Encoding.UTF8.GetBytes(_appSettings.SecretKey);

//            var parameter = new TokenValidationParameters
//            {
//                ValidateIssuerSigningKey = true,
//                ValidateIssuer = false, // có thể bật nếu bạn có Issuer
//                ValidateAudience = false, // có thể bật nếu bạn có Audience

//                ClockSkew = TimeSpan.Zero,
//                IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),
 
//               ValidateLifetime =false,
//            };
//            try
//            {
//                Check1 : AccessToken valid format
//                var tokenInVerification = jwtTokenHandler.ValidateToken(model.AccessToken, parameter,
//                    out var validatedToken);
//                check2 : check alg
//                if (validatedToken is JwtSecurityToken jwtSecurityToken)
//                {
//                    var result = jwtSecurityToken.Header.Alg.Equals(
//                        SecurityAlgorithms.HmacSha512,
//                        StringComparison.InvariantCultureIgnoreCase);
//                    if (!result)
//                    {
//                        return Ok(new 
//                        {
//                            Success = false,
//                            Message = "Invalid token"
//                        });
//                    }else
//                    {
//                        return Ok(new
//                        {
//                            Success = true,
//                        });
//                    }

//                }

//                Check4 
                
//                var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);
//                if(utcExpireDate != null)
//                {
//                    var expireDate = ConvertUnixTimeToDataTime(utcExpireDate);
//                    if (expireDate > DateTime.UtcNow)
//                    {
//                        return Ok(new
//                        {
//                            Success = false,
//                            Message = "Access token has not yet expired"
//                        });
//                    }
//                }
                
//                check4: Check refreshtoken exitst in DB
//                var storedToken = _dbLoaiContext.refreshTokens.FirstOrDefault
//                    (x => x.Token == model.RefreshToken);
//                if (storedToken == null)
//                {
//                    return Ok(new
//                    {
//                        Success = false,
//                        Meassage = "Refresh toke does"

//                    });
//                }
//                check 
//                if (storedToken.IsUsed)
//                {
//                    return Ok(new
//                    {
//                        Success = false,
//                        Meassage = "IsUser"

//                    });
//                }
//                if (storedToken.IsRevoked)
//                {
//                    return Ok(new
//                    {
//                        Success = false,
//                        Meassage = "IsRevoked"

//                    });
//                }

//                check 6: AccessToken id == JwtId in RefreshToken
//                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
//                if (storedToken.JwtId != jti)
//                {
//                    return Ok(new 
//                    {
//                        Success = false,
//                        Message = "Token doesn't match"
//                    });
//                }

//                Update token is used
//                storedToken.IsRevoked = true;
//                storedToken.IsUsed = true;
//                _dbLoaiContext.Update(storedToken);
//                await _dbLoaiContext.SaveChangesAsync();

//                create new token
//                var user = await _dbLoaiContext.userModels.SingleOrDefaultAsync(nd => nd.id == storedToken.UserId);
//                var token = GenerateToken(user);

//                return Ok(new 
//                {
//                    Success = true,
//                    Message = "Renew token success",
//                    Data = token
//                });

//            }
//            catch
//            {
//                return BadRequest(new
//                {
//                    Success = false,
//                    Message = "Loi"
//                });

//            }
//        }

//        private DateTime ConvertUnixTimeToDataTime(long utcExpireDate)
//        {
//            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
//            dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();
//            return dateTimeInterval;
//        }
//    }
//}
