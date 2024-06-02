using FirebirdSql.Data.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;

namespace React.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculatronController : ControllerBase
    {
        //public async Task<(bool result, bool hasConnect)> LogIn()
        //{
        //    var postData = JsonConvert.SerializeObject(new UserData() { login = loginServer, password = passwordServer });

        //    var hasConnect = true;
        //    HttpWebRequest request;
        //    HttpWebResponse response = null;

        //    try
        //    {
        //        request = (HttpWebRequest)WebRequest.Create($"http://{Ip}:{port}/api/user/login");
        //        request.Method = "POST";
        //        byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);
        //        request.ContentType = "application/json";
        //        request.ContentLength = byteArray.Length;
        //        using (Stream dataStream = request.GetRequestStream())
        //        {
        //            dataStream.Write(byteArray, 0, byteArray.Length);
        //        }

        //        response = (HttpWebResponse)await request.GetResponseAsync();
        //        using (Stream stream = response.GetResponseStream())
        //        {
        //            if (response.StatusCode == HttpStatusCode.OK)
        //            {
        //                using (StreamReader reader = new StreamReader(stream))
        //                {
        //                    cookie = JsonConvert.DeserializeObject<TokenStruct>(reader.ReadToEnd());
        //                }

        //                if (!string.IsNullOrEmpty(cookie.accessToken))
        //                    return (true, hasConnect);
        //            }
        //            else
        //            {
        //                throw new WebException($"Код ответа: {response.StatusCode}");
        //            }
        //        }
        //    }
        //    catch (WebException ex)
        //    {
        //        //LoggerService.Error(ex, "Ошибка авторизации", this);
        //        if (ex.Status == WebExceptionStatus.ConnectFailure)
        //        {
        //            hasConnect = false;
        //        }
        //    }
        //    finally
        //    {
        //        response?.Close();
        //    }

        //    return (false, hasConnect);
        //}

        //public async Task<bool> StartTaskShedulers(int taskId)
        //{
        //    HttpWebRequest request;
        //    HttpWebResponse response = null;

        //    if (!IsAuthorized) throw new Exception("Пользователь не авторизован в компоненте");

        //    try
        //    {
        //        request = (HttpWebRequest)WebRequest.Create($"http://{Ip}:{port}/api/remoted/{taskId}/true/changetaskshedulers");

        //        //Добавляю аутентификационный токен в заголовок запроса
        //        request.Headers.Set("X-CALC-AUTH", EncodeURIComponentJsAnalog(cookie.accessToken));

        //        response = (HttpWebResponse)await request.GetResponseAsync();
        //        using (Stream stream = response.GetResponseStream())
        //        {
        //            if (response.StatusCode == HttpStatusCode.OK)
        //            {
        //                using (StreamReader reader = new StreamReader(stream))
        //                {
        //                    return Convert.ToBoolean(reader.ReadToEnd());
        //                }
        //            }
        //            else
        //            {
        //                throw new Exception($"Код ответа: {response.StatusCode}");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LoggerService.Error(ex, "Ошибка запуска планировщика", this);
        //    }
        //    finally
        //    {
        //        response?.Close();
        //    }

        //    return false;
        //}
        
        //public async Task<bool> StopTaskShedulers(int taskId)
        //{
        //    HttpWebRequest request;
        //    HttpWebResponse response = null;

        //    if (!IsAuthorized) throw new Exception("Пользователь не авторизован в компоненте");

        //    try
        //    {
        //        request = (HttpWebRequest)WebRequest.Create($"http://{Ip}:{port}/api/remoted/{taskId}/false/changetaskshedulers");

        //        //Добавляю аутентификационный токен в заголовок запроса
        //        request.Headers.Set("X-CALC-AUTH", EncodeURIComponentJsAnalog(cookie.accessToken));

        //        response = (HttpWebResponse)await request.GetResponseAsync();
        //        using (Stream stream = response.GetResponseStream())
        //        {
        //            if (response.StatusCode == HttpStatusCode.OK)
        //            {
        //                using (StreamReader reader = new StreamReader(stream))
        //                {
        //                    return Convert.ToBoolean(reader.ReadToEnd());
        //                }
        //            }
        //            else
        //            {
        //                throw new Exception($"Код ответа: {response.StatusCode}");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LoggerService.Error(ex, "Ошибка останова планировщика", this);
        //    }
        //    finally
        //    {
        //        response?.Close();
        //    }

        //    return false;
        //}

        //private async Task<bool> Ping()
        //{
        //    HttpWebRequest request;
        //    HttpWebResponse response = null;

        //    if (!IsAuthorized) throw new Exception("Пользователь не авторизован в компоненте");

        //    try
        //    {
        //        request = (HttpWebRequest)WebRequest.Create($"http://{Ip}:{port}");

        //        response = (HttpWebResponse)await request.GetResponseAsync();

        //        return response.StatusCode == HttpStatusCode.OK;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //    finally
        //    {
        //        response?.Close();
        //    }
        //}
    }
}
