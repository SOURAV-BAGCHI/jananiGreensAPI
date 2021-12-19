using System;
using System.IO;
using System.Text;
using Microsoft.IdentityModel.Protocols;
using MimeKit;
using MailKit.Net.Smtp;
using System.Collections.Generic;
using Models;

namespace CommonMethodLib
{
    public class CommonMethod
    {
        
        public static String ReadHtmlFile(String htmlFilePath)
        {
            StringBuilder store = new StringBuilder();

            try
            {
                using (StreamReader htmlReader = new StreamReader(htmlFilePath))
                {
                    String line;
                    while ((line = htmlReader.ReadLine()) != null)
                    {
                        store.Append(line);
                    }
                }
            }
            // catch (Exception ex)
            catch (Exception)
             { }

            return store.ToString();
        }

        public static bool SendMail(String SendereMailId,String Pwd, String Mailbody, String EmailAddresses, String Subject,String Host,Int32 Port=2525,Boolean UseSSl=false)//, String MailAttachmentFilename, String TempImageFileName)
        {
            Boolean success = false;
            if(EmailAddresses=="NONE")
            {
                return false;
            }
            try
            {
                
                MimeMessage message = new MimeMessage();

                MailboxAddress from = new MailboxAddress("Janani Greens", 
                SendereMailId);
                message.From.Add(from);

                MailboxAddress to = new MailboxAddress("User", 
                EmailAddresses);
                message.To.Add(to);

                message.Subject = Subject;

                BodyBuilder bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = Mailbody;
                //bodyBuilder.TextBody = "Hello World!";

                message.Body = bodyBuilder.ToMessageBody();

                MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient();
                client.Connect(Host, Port, UseSSl);
                client.Authenticate(SendereMailId, Pwd);

                client.Send(message);
                client.Disconnect(true);
                client.Dispose();

                success = true;
            }
            // catch (Exception ex)
            catch (Exception)
            {

            }
            return success;
        }

        public static List<RoomModel> ModifyRoomDetailsAccordingToMonths(List<RoomModel> RDList)
        {

            DateTime dateTime=DateTime.Now;

            if((dateTime.Month==12) || (dateTime.Month==1) || (dateTime.Month==2) || (dateTime.Month==3))
            {
                foreach(var m in RDList)
                {
                    m.RatePerDay+=m.ACCharges;
                    m.ACCharges=0;
                }
            }
            return RDList;
        }

        public static RoomModel ModifyRoomDetailsAccordingToMonths(RoomModel RDObj)
        {

            DateTime dateTime=DateTime.Now;

            if((dateTime.Month==12) || (dateTime.Month==1) || (dateTime.Month==2) || (dateTime.Month==3))
            {
                
                    RDObj.RatePerDay+=RDObj.ACCharges;
                    RDObj.ACCharges=0;
                
            }
            return RDObj;
        }
    }
}