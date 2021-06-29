using System;
using System.Text;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using MySql.Data.MySqlClient;

namespace TwitterBot
{
    class Twitter
    {
        private const String oauth_consumer_key = "0mIHDRnu7eSzTRVuVUJ8oTOVK";
        private const String oauth_consumer_secret = "lx8zBqQAw0mH0m5YtVhjCdxdbXNHuXqvvPPM9sqMfc2LssJ4zt";
        private const String oauth_token = "1278507980024053762-OCcxsr5TT8r6MVgfnSSZ9UoeIsHLYS";
        private const String oauth_token_secret = "1sMCxQsrbDK2Bh1orajTctbVB80jhAzdpUVL6yJSlEV2b";
        private const String oauth_version = "1.0";
        private const String oauth_signature_method = "HMAC-SHA1";
        private const String dataBaseInfo = @"server=ticketstalamas.com;userid=tickets2_admin;password=ticketstalamas01;database=tickets2_bot";
        Mail mail = new Mail("jhzeg1696@mail.com", "Catastros", "jhzeg1696@gmail.com");
        public void Tweet(String movieTitle, String movieOverview, String movieReleaseDate, long imageId)
        {
    
            string twitterURL = "https://api.twitter.com/1.1/statuses/update.json";
            String message = $"{movieOverview}. \n \n \"{movieTitle}\"";      

            // create unique request details
            string oauth_nonce = Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
            System.TimeSpan timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));
            string oauth_timestamp = Convert.ToInt64(timeSpan.TotalSeconds).ToString();

            // create oauth signature
            string baseFormat = "media_ids={7}&oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}" + "&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}&status={6}";

            string baseString = string.Format(
                baseFormat,
                oauth_consumer_key,
                oauth_nonce,
                oauth_signature_method,
                oauth_timestamp,
                oauth_token,
                oauth_version,
                Uri.EscapeDataString(message),
                imageId
            ); ;

            string oauth_signature = null;
            using (HMACSHA1 hasher = new HMACSHA1(ASCIIEncoding.ASCII.GetBytes(Uri.EscapeDataString(oauth_consumer_secret) + "&" + Uri.EscapeDataString(oauth_token_secret))))
            {
                oauth_signature = Convert.ToBase64String(hasher.ComputeHash(ASCIIEncoding.ASCII.GetBytes("POST&" + Uri.EscapeDataString(twitterURL) + "&" + Uri.EscapeDataString(baseString))));
            }

            // create the request header
            string authorizationFormat = "OAuth oauth_consumer_key=\"{0}\", oauth_nonce=\"{1}\", " + "oauth_signature=\"{2}\", oauth_signature_method=\"{3}\", " + "oauth_timestamp=\"{4}\", oauth_token=\"{5}\", " + "oauth_version=\"{6}\"";

            string authorizationHeader = string.Format(
                authorizationFormat,
                Uri.EscapeDataString(oauth_consumer_key),
                Uri.EscapeDataString(oauth_nonce),
                Uri.EscapeDataString(oauth_signature),
                Uri.EscapeDataString(oauth_signature_method),
                Uri.EscapeDataString(oauth_timestamp),
                Uri.EscapeDataString(oauth_token),
                Uri.EscapeDataString(oauth_version)
            );

            HttpWebRequest objHttpWebRequest = (HttpWebRequest)WebRequest.Create(twitterURL);
            objHttpWebRequest.Headers.Add("Authorization", authorizationHeader);
            objHttpWebRequest.Method = "POST";
            objHttpWebRequest.ContentType = "application/x-www-form-urlencoded";
            //TODO: String media_id = "1280025720576790535";

            using (Stream objStream = objHttpWebRequest.GetRequestStream())
            {
                byte[] content = ASCIIEncoding.ASCII.GetBytes("media_ids=" + imageId + "&status=" + Uri.EscapeDataString(message));
                objStream.Write(content, 0, content.Length);

            }

            var responseResult = "";

            try
            {
                //success posting
                WebResponse objWebResponse = objHttpWebRequest.GetResponse();
                StreamReader objStreamReader = new StreamReader(objWebResponse.GetResponseStream());
                responseResult = objStreamReader.ReadToEnd().ToString();
                Console.Write("Se twiteo correctamente");
                mail.SendMail("Jefe, acabo de twittear", "Bot");
            }
            catch (Exception ex)
            {
                responseResult = "Twitter Post Error: " + ex.Message.ToString() + ", authHeader: " + authorizationHeader;
                mail.SendMail("Jefe ocurrio un error y no pude twittear, el error es el siguiente: \n" + ex.Message.ToString(), "Bot error");
            }
        }

        public void BuildAndSendTweet()
        {
            String movieTitle = "";
            String movieOverview = "";
            String movieReleaseDate = "";
            String movieOverviewSubString = "";
            long movieImg = 0;
            using var connection = new MySqlConnection(dataBaseInfo);
            connection.Open();
            string query = "SELECT * FROM random_movie WHERE id = 1";
            using var command = new MySqlCommand(query, connection);

            using MySqlDataReader dataReader = command.ExecuteReader();

            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    movieTitle = dataReader.GetString(1);
                    movieOverview = dataReader.GetString(2);
                    if(movieOverview.Length < 1)
                    {
                        movieOverviewSubString = "Lo siento, no cuento con la descripción de esta película pero deberías darle una oportunidad!";
                    }
                    if(movieOverview.Length < 10)
                    {
                        movieOverviewSubString = "Lo siento, no cuento con la descripción de esta película pero deberías darle una oportunidad!";
                    }
                    else
                    {
                        movieOverviewSubString = movieOverview.Substring(0, movieOverview.IndexOf("."));
                        if(movieOverviewSubString.Length > 220)
                        {
                            movieOverviewSubString = "Lo siento, no cuento con la descripción de esta película pero deberías darle una oportunidad!";
                        }
                    }
                    movieReleaseDate = dataReader.GetString(3);
                    movieImg = dataReader.GetInt64(4);

                }
                Tweet(movieTitle, movieOverviewSubString, movieReleaseDate, movieImg);
            }

            else
            {
                String error = "Ocurrio un error al tratar de obtener los datos mysql";
                mail.SendMail("Jefe ocurrio un error y no pude twittear, el error es el siguiente: \n" + error, "Bot error");

            }
            connection.Close();
        }
    }
}
