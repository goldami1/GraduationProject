﻿using System;
using System.Net;
using System.IO;
//using System.Web.Script.Serialization;
//using System.Net.Http;
using System.Json;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;


namespace AgentVIProxy
{
    public class User
    {
        public string Username { get; set; }
        public string AccessToken { get; set; }
        public InnoviObjectCollection<Account> Accounts { get; set; }

        private static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static LoginResult Login(string i_Email, string i_Password)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.innovi.agentvi.com/api/user/login");
            request.Method = "POST";
            request.Headers.Add("X-API-KEY:" + Settings.ApiKey);
            request.ContentType = "application/json";
            request.Accept = "application/json";

            // Must change this to accept only this domain without a certificate
      //            System.Net.ServicePointManager.ServerCertificateValidationCallback =
       //               ((sender, certificate, chain, sslPolicyErrors) => true);

            System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);


            Dictionary<string, JsonValue> jsonBuilder = new Dictionary<string, JsonValue>();
            jsonBuilder.Add("email", i_Email);
            jsonBuilder.Add("password", i_Password);
            string json = new JsonObject(jsonBuilder).ToString();

            using (StreamWriter streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            HttpStatusCode status = response.StatusCode;
            WebHeaderCollection collection = response.Headers;


            string accessToken = response.Headers["x-access-token"];

            JsonValue responseBody = null;

            using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
            {
                string responseStream = streamReader.ReadToEnd();
                responseBody = JsonObject.Parse(responseStream);
            }

            HttpStatusCode statusCode = response.StatusCode;    ///////////////////////////////////////
            // throw exception if code is not OK

            eErrorMessage errorMessage = eErrorMessage.Empty;

            if (responseBody["error"] != null)
            {
                errorMessage = eErrorMessage.ServerError;
            }
           
            LoginResult loginResult = new LoginResult();
            loginResult.ErrorMessage = errorMessage;
            

            if (errorMessage == eErrorMessage.Empty)
            {
                // change to fetch real data
                User user = createDummyUser();
                user.AccessToken = accessToken;
                loginResult.User = user;
            }
          
            return loginResult;
        }

        private static User createDummyUser()
        {
            User dummyUser = new User();

            Sensor dummySensor1 = new Sensor();
            dummySensor1.Name = "Dummy Camera 1";
            dummySensor1.SensorStatus = eSensorStatus.Active;
            dummySensor1.SensorType = eSensorType.CCD;
            dummySensor1.IsEnabledByUser = true;
            dummySensor1.IsRecording = true;
            Dictionary<string, Sensor> dummySensors = new Dictionary<string, Sensor>();
            dummySensors.Add(dummySensor1.Name, dummySensor1);

            Sensor dummySensor2 = new Sensor();
            dummySensor2.Name = "Dummy Camera 2";
            dummySensor2.SensorStatus = eSensorStatus.Inactive;
            dummySensor2.SensorType = eSensorType.Thermal;
            dummySensors.Add(dummySensor2.Name, dummySensor2);


            SiteFolder dummySiteFolder = new SiteFolder();
            dummySiteFolder.Name = "Dummy Site";
            dummySiteFolder.Sensors = new InnoviObjectCollection<Sensor>(dummySensors);
            Dictionary<string, SiteFolder> dummySiteFolders = new Dictionary<string, SiteFolder>();
            dummySiteFolders.Add(dummySiteFolder.Name, dummySiteFolder);

            CustomerFolder dummyCustomerFolder = new CustomerFolder();
            dummyCustomerFolder.Name = "Dummy Customer";
            dummyCustomerFolder.SiteFolders = new InnoviObjectCollection<SiteFolder>(dummySiteFolders);
            Dictionary<string, CustomerFolder> dummyCustomerFolders = new Dictionary<string, CustomerFolder>();
            dummyCustomerFolders.Add(dummyCustomerFolder.Name, dummyCustomerFolder);

            Account dummyAccount = new Account();
            dummyAccount.Name = "Dummy Account";
            dummyAccount.Status = eAccountStatus.Active;
            dummyAccount.CustomerFolders = new InnoviObjectCollection<CustomerFolder>(dummyCustomerFolders);
            Dictionary<string, Account> dummyAccounts = new Dictionary<string, Account>();
            dummyAccounts.Add(dummyAccount.Name, dummyAccount);

            dummyUser.Username = "Ami";
            dummyUser.Accounts = new InnoviObjectCollection<Account>(dummyAccounts);

            return dummyUser;
        }
    }
}