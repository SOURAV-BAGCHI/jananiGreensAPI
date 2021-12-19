using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CommonMethodLib;
using Data;
using DataLib;
using Helpers;
using JANANIGREENS.API.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models;
using Models.ClientViewModel;
using Models.ViewModels;
using Newtonsoft.Json;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController:ControllerBase
    {
        private ApplicationDbContext _db;
        private readonly AppSettings _appSettings;
        private readonly IDataMethods _dm;
        private readonly IWebHostEnvironment _env;
        
        public AdminController(ApplicationDbContext db,IOptions<AppSettings> appSettings,
        IDataMethods dm,IWebHostEnvironment env)
        {
            _db=db;
            _appSettings=appSettings.Value;
            _dm=dm;
            _env=env;
        }

        private enum Status{
            Cancel,
            Verified,
            PaymentDone,
            Checkedin,
            Checkedout
        }

        [HttpGet("[action]")]
        // [Authorize(Policy="RequireAdministratorRole")]
        public IActionResult GetBookingList()
        {
            var BookingList=_db.BookingDetailsAndStatus
                            .Where(p=>p.IsProcessComplete==false)
                            .Select(p=> new BookingDetailsAndStatusViewModel()
                                { BookingId=p.BookingId,
                                BookingStartDate=p.BookingStartDate.ToString("dd-MM-yyyy"),
                                // BookingStartDate=p.BookingStartDate,
                                  CustomerName=p.CustomerName,
                                  Status=p.Status
                                });

            return Ok(BookingList);
        }

        [HttpGet("[action]")]
        public IActionResult GetCancellationRequestList()
        {
            var BookingList=_db.CancellationRequests
                            .Where(p=>p.CancellationAccepted==false)
                            .Select(p=> new CancellationRequestViewModel()
                                { BookingRequestId=p.BookingRequestId,
                                    BookingStartDate=p.BookingStartDate.ToString("dd-MM-yyyy"),
                                    CustomerName=p.CustomerName,
                                    CancellationRequestDate=p.RequestDateTime.ToString("dd-MM-yyyy"),
                                    Reason=p.Reason
                                });

            return Ok(BookingList);
        }
        [HttpPut("[action]")]
        // [Authorize(Policy="RequireAdministratorRole")]
        public async Task<IActionResult> UpdateBookingList([FromBody] BookingDetailsUpdateViewModel formData)
        {
            List<StatusRecordViewModel> SRVMObjList=new List<StatusRecordViewModel>();
           
            var data=await _db.BookingDetailsAndStatus.FindAsync(formData.BookingId);
            Int64 bookingId=Int64.Parse(formData.BookingId);
            if(data!=null)
            {
                SRVMObjList=JsonConvert.DeserializeObject<List<StatusRecordViewModel>>(data.StatusList);
                SRVMObjList.Add(
                    new StatusRecordViewModel(){
                        Status=formData.Status,
                        StatusDate=DateTime.Now.ToString("dd-MM-yyyy HH:mm")
                    }
                );

                data.StatusList=JsonConvert.SerializeObject(SRVMObjList);
                 // Add record to current room booking details
                var RoomOrderDetails=_db.BookingRequestQueue
                                        .Where(p=>p.BookingRequestId==formData.BookingId).FirstOrDefault();
                                    //    .Select(p=> new{p.RoomOrderDetails,p.BookingStartDate,p.BookingEndDate}).FirstOrDefault();
                    
                switch(formData.Status)
                {
                    case (short)Status.Cancel:
                        if(data.Status==(short)Status.PaymentDone)
                        {
                            // booking gets deleted from current room booking details
                            var roomBookingList=_db.CurrentRoomBookingDetails.Where(p=>p.FkBookingId==bookingId);
                            foreach(var roomBooking in roomBookingList)
                            {
                                _db.CurrentRoomBookingDetails.Remove(roomBooking);
                            }
                        }

                        if(data.Status<=(short)Status.PaymentDone)
                        {
                            // status becomes 0 
                            data.Status=(short)Status.Cancel;
                            // iscomplete becomes true    
                            data.IsProcessComplete=true;
                        }
                        
                        if(formData.AdditionalDetails=="REQUESTCANCELLATION")
                        {
                            var cancellationReq=await _db.CancellationRequests.FindAsync(formData.BookingId);
                            if(cancellationReq!=null)
                            {
                                cancellationReq.CancellationAccepted=true;
                                _db.Entry(cancellationReq).State=EntityState.Modified;
                            }
                        }
                        
                        _db.Entry(data).State=EntityState.Modified;
                        _db.SaveChanges();

                        var newdata101=data;
                        newdata101.StatusList=RoomOrderDetails.Email;
                        var Obj101= JsonConvert.SerializeObject(newdata101);
                        // var Obj= JsonSerializer.Serialize<BookingRequestQueueModel>(formData);

                        ParameterizedThreadStart parameterizedThreadStartSendCancellationMail = new ParameterizedThreadStart(SendCancellationMail);
                        Thread Thread202 = new Thread(parameterizedThreadStartSendCancellationMail);
                        Thread202.Start(Obj101);

                    break;
                    case (short)Status.PaymentDone:
                        
                        List<RoomOrderBasicViewModel> ROBVMObjList=JsonConvert.DeserializeObject<List<RoomOrderBasicViewModel>>(RoomOrderDetails.RoomOrderDetails);
                        List<Object> RoomList=new List<Object>();

                        foreach(var roomorderdetails in ROBVMObjList)
                        {
                            var CurrentRoomBookingDetail=new CurrentRoomBookingDetailsModel()
                            {
                                FkRoomId=roomorderdetails.RoomId,
                                FkBookingId=bookingId,
                                StartDate=RoomOrderDetails.BookingStartDate,
                                EndDate=RoomOrderDetails.BookingEndDate
                            };
                            RoomList.Add(new{RoomId=roomorderdetails.RoomId});

                            await  _db.CurrentRoomBookingDetails.AddAsync(CurrentRoomBookingDetail);
                        }
                        
                        // status becomes 1
                        data.Status=(short)Status.PaymentDone;

                        _db.Entry(data).State=EntityState.Modified;
                        _db.SaveChanges();

                        /**************************************************************/
                            SqlParameter param1 = new SqlParameter("@str_BookingRequestId", bookingId); 
                            SqlParameter param2 = new SqlParameter("@startDate", RoomOrderDetails.BookingStartDate); 
                            SqlParameter param3 = new SqlParameter("@endDate", RoomOrderDetails.BookingEndDate);
                            SqlParameter param4 = new SqlParameter("@@str_RoomList", JsonConvert.SerializeObject(RoomList));

                        var cancelledRequest =  _db.Set<CancelBookingRequestViewModel>().FromSqlRaw("usp_BookingRequest_CancelOtherBooking {0},{1},{2},{3}",param1,param2,param3,param4).ToList();

                        if(cancelledRequest.Count>0)
                        {
                            var cancelledlistobj=JsonConvert.SerializeObject(cancelledRequest);
                            ParameterizedThreadStart parameterizedThreadStartSendRequestCancellationMail = new ParameterizedThreadStart(SendRequestCancellationMail);
                            Thread ThreadSendRequestCancellationMail = new Thread(parameterizedThreadStartSendRequestCancellationMail);
                            ThreadSendRequestCancellationMail.Start(cancelledlistobj);
                        }
                        /**************************************************************/

                        var Obj= JsonConvert.SerializeObject(RoomOrderDetails);
                        // var Obj= JsonSerializer.Serialize<BookingRequestQueueModel>(formData);

                        ParameterizedThreadStart parameterizedThreadStartSendAdvanceReceivedMail = new ParameterizedThreadStart(SendAdvanceReceivedMail);
                        Thread Thread1 = new Thread(parameterizedThreadStartSendAdvanceReceivedMail);
                        Thread1.Start(Obj);

                    break;
                    case (short)Status.Checkedin:
                        
                        //checkin time updated
                        data.Checkin=DateTime.Now;

                        // status changed to checkedin
                        data.Status=(short)Status.Checkedin;

                        _db.Entry(data).State=EntityState.Modified;
                        _db.SaveChanges();

                        var newdata1=data;
                        newdata1.StatusList=RoomOrderDetails.Email;
                        var Obj11= JsonConvert.SerializeObject(newdata1);
                        // var Obj= JsonSerializer.Serialize<BookingRequestQueueModel>(formData);

                        ParameterizedThreadStart parameterizedThreadStartSendCheckinMail = new ParameterizedThreadStart(SendCheckinMail);
                        Thread Thread22 = new Thread(parameterizedThreadStartSendCheckinMail);
                        Thread22.Start(Obj11);
                    break;
                    case (short)Status.Checkedout:

                        // booking gets deleted from current room booking details
                        var roomBookingList1=_db.CurrentRoomBookingDetails.Where(p=>p.FkBookingId==bookingId);
                        foreach(var roomBooking in roomBookingList1)
                        {
                            _db.CurrentRoomBookingDetails.Remove(roomBooking);
                        }
                        // checkout time updated
                        data.Checkout=DateTime.Now;
                        // status updated to checkout
                        data.Status=(short)Status.Checkedout;
                        // process complete updated
                        data.IsProcessComplete=true;

                        _db.Entry(data).State=EntityState.Modified;
                        _db.SaveChanges();

                        var newdata=data;
                        newdata.StatusList=RoomOrderDetails.Email;
                        var Obj2= JsonConvert.SerializeObject(newdata);
                        // var Obj= JsonSerializer.Serialize<BookingRequestQueueModel>(formData);

                        ParameterizedThreadStart parameterizedThreadStartSendCheckoutMail = new ParameterizedThreadStart(SendCheckoutMail);
                        Thread Thread2 = new Thread(parameterizedThreadStartSendCheckoutMail);
                        Thread2.Start(Obj2);
                    break;
                }

            }
            else
            {
                return BadRequest();
            }

            return Ok(1);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> GenerateBooking([FromBody] BookingRequestViewModel formData)
        {
            List<RoomOrderBasicViewModel> ROBVMObjList=new List<RoomOrderBasicViewModel>();
            Int32 TotalNoOfDays=0;
            CultureInfo provider = CultureInfo.InvariantCulture;  
            DateTime StartDate=DateTime.ParseExact(formData.BookingStartDate, "dd-MM-yyyy HH:mm", provider);
            DateTime EndDate=DateTime.ParseExact(formData.BookingEndDate, "dd-MM-yyyy HH:mm", provider);
            String BookingRequestId=DateTime.Now.Ticks.ToString();
            formData.BookingRequestId=BookingRequestId;
            
            var RoomDetails=JsonConvert.DeserializeObject<RoomDetailsBasicsViewModel[]>(formData.RoomOrderDetails);

            TotalNoOfDays=Convert.ToInt32((EndDate-StartDate).TotalDays);

            foreach(var m in RoomDetails)
            {
                var TempRoomDetails= await _db.Rooms.FindAsync(m.RoomId);

                if(m.IsAcAvailed)
                {
                    TempRoomDetails.IsACAvailable=true;
                    TempRoomDetails.ACCharges=TempRoomDetails.ACCharges;
                }
                else
                {
                    TempRoomDetails.IsACAvailable=false;
                    TempRoomDetails.ACCharges=0;
                }

                var TempObj=new RoomOrderBasicViewModel()
                {
                    RoomId=TempRoomDetails.RoomId,
                    Name=TempRoomDetails.Name,
                    IsAcAvailed=TempRoomDetails.IsACAvailable,
                    NumberOfPerson=m.NumberOfPersons,
                    RatePerDay=TempRoomDetails.RatePerDay,
                    ACCharges=TempRoomDetails.ACCharges,
                    Image=JsonConvert.DeserializeObject<String[]>(TempRoomDetails.ImageList)[0]
                };


                ROBVMObjList.Add(TempObj);

            }


            var newformData=new BookingRequestQueueModel(){
                BookingRequestId=formData.BookingRequestId,
                Name=formData.Name,
                Phone=formData.Phone,
                Email=formData.Email,
                BookingStartDate=StartDate,
                BookingEndDate=EndDate,
                RoomOrderDetails=JsonConvert.SerializeObject(ROBVMObjList),
                VerificationCode=Int64.Parse(formData.BookingRequestId),
                VerificationLimit=DateTime.Now.AddHours(_appSettings.VerificationTimeLimitInHrs),
                IsVerified=true,
                CreateDate=DateTime.Now
            };


            await _db.BookingRequestQueue.AddAsync(newformData);

            var Obj= JsonConvert.SerializeObject(newformData);
            // await _db.SaveChangesAsync();
            _dm.InitiateBookingDetailsAndStatus(Obj,_db);


            return Ok(1);

        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetBookingHistoryCount()
        {
            var Count=await _db.BookingDetailsAndStatus.
            Where(x=>//x.IsProcessComplete==true 
                //&& 
                x.Status==(Int16)Status.Checkedout).
            CountAsync();

            return Ok(Count);
        }

        [HttpGet("[action]/{lastrecord}")]
        public IActionResult GetBookingHistory([FromRoute] String lastrecord)
        {
            // CultureInfo provider = CultureInfo.InvariantCulture;  
            // if(lastrecord=="START")
            // {
            //     var BookingList=_db.BookingDetailsAndStatus
            //                 .Where(p=>p.IsProcessComplete==true && p.Status==(Int16)Status.Checkedout)
            //                 .OrderByDescending(p=>p.BookingStartDate)
            //                 .Take(_appSettings.PaginationRecordCount)
            //                 .Select(p=> new BookingDetailsAndStatusViewModel()
            //                     {   BookingId=p.BookingId,
            //                         BookingStartDate=p.BookingStartDate.ToString("dd-MM-yyyy HH:mm"),
            //                         CustomerName=p.CustomerName,
            //                         Status=p.Status
            //                     });

            //     return Ok(BookingList);
            // }
            // else
            // {
            //     DateTime dateTimestartdt = DateTime.ParseExact(lastrecord, "dd-MM-yyyy HH:mm", provider);

            //     var BookingList=_db.BookingDetailsAndStatus
            //                 .Where(p=>p.IsProcessComplete==true && p.Status==(Int16)Status.Checkedout && p.BookingStartDate>dateTimestartdt)
            //                 .OrderByDescending(p=>p.BookingStartDate)
            //                 .Take(_appSettings.PaginationRecordCount)
            //                 .Select(p=> new BookingDetailsAndStatusViewModel()
            //                     {   BookingId=p.BookingId,
            //                         BookingStartDate=p.BookingStartDate.ToString("dd-MM-yyyy"),
            //                         CustomerName=p.CustomerName,
            //                         Status=p.Status
            //                     });

            //     return Ok(BookingList);
            // }
            

                var BookingList=_db.BookingDetailsAndStatus
                    .Where(p=> p.Status==(Int16)Status.Checkedout)
                    .OrderByDescending(p=>p.BookingStartDate)
                    // .Take(_appSettings.PaginationRecordCount)
                    .Select(p=> new BookingDetailsAndStatusViewModel()
                        {   BookingId=p.BookingId,
                            BookingStartDate=p.BookingStartDate.ToString("dd-MM-yyyy HH:mm"),
                            CustomerName=p.CustomerName,
                            Status=p.Status
                        });

                return Ok(BookingList);
        }

        [HttpGet("[action]/{pageno}")]
        public IActionResult GetBookingHistory2([FromRoute] Int32 pageno)
        {
            SqlParameter param1 = new SqlParameter("@i_NextRowsCount", _appSettings.PaginationRecordCount); 
            SqlParameter param2 = new SqlParameter("@i_PageNo", pageno);

            var BookingList =  _db.Set<BookingDetailsAndStatusViewModel>().FromSqlRaw("usp_BookingDetailsAndStatus_GetBookingCompleted {0},{1}",param1,param2).ToList();

            return Ok(BookingList);
        }

        private void SendAdvanceReceivedMail(Object Obj)
        {
            BookingRequestQueueModel formData=JsonConvert.DeserializeObject<BookingRequestQueueModel>(Obj.ToString());
            List<RoomOrderBasicViewModel> ROBVMObjList=JsonConvert.DeserializeObject<List<RoomOrderBasicViewModel>>(formData.RoomOrderDetails);

            String AbsolutePath=Path.Combine(_env.WebRootPath,"Images/");
            String contentRootPath = _env.ContentRootPath;

            String path = Path.Combine(contentRootPath , "Content","EmailTemplates","Payment_Received_Confirmation.html");
            String EmailTemplate = String.Empty;
            EmailTemplate = CommonMethod.ReadHtmlFile(path);

            EmailTemplate = EmailTemplate.Replace("@@BOOKINGSTATUS@@", "Advanced Received");
            EmailTemplate = EmailTemplate.Replace("@@BOOKINGID@@", formData.BookingRequestId);
            EmailTemplate = EmailTemplate.Replace("@@USERNAME@@", formData.Name);

            Double TotalCost=0.0;
            var TotalNoOfDays=Convert.ToInt32((formData.BookingEndDate-formData.BookingStartDate).TotalDays);
            Int32 Counter=1;
            var builder = new StringBuilder();
            using (var xmlwriter = XmlWriter.Create(builder))
            {
                xmlwriter.WriteStartElement("table");
                xmlwriter.WriteAttributeString("id","customers");

                    xmlwriter.WriteStartElement("tr");
                        xmlwriter.WriteStartElement("th");
                            xmlwriter.WriteString("S.No.");
                        xmlwriter.WriteEndElement();
                        
                        xmlwriter.WriteStartElement("th");
                            xmlwriter.WriteString("Room");
                        xmlwriter.WriteEndElement();
                    
                        xmlwriter.WriteStartElement("th");
                            xmlwriter.WriteString("Nights");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("th");
                            xmlwriter.WriteString("No. of persons");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("th");
                            xmlwriter.WriteString("Currency");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("th");
                            xmlwriter.WriteString("Amount");
                        xmlwriter.WriteEndElement();

                    xmlwriter.WriteEndElement();
                    foreach (var ItemObj in ROBVMObjList)
                    {
                        xmlwriter.WriteStartElement("tr");
                            
                            xmlwriter.WriteStartElement("td");
                                xmlwriter.WriteString((Counter++).ToString());
                            xmlwriter.WriteEndElement();

                            xmlwriter.WriteStartElement("td");
                                xmlwriter.WriteString(ItemObj.Name);
                            xmlwriter.WriteEndElement();

                            xmlwriter.WriteStartElement("td");
                                xmlwriter.WriteString((TotalNoOfDays).ToString());
                            xmlwriter.WriteEndElement();

                            xmlwriter.WriteStartElement("td");
                                xmlwriter.WriteString((ItemObj.NumberOfPerson).ToString());
                            xmlwriter.WriteEndElement();

                            xmlwriter.WriteStartElement("td");
                                xmlwriter.WriteString("INR");
                            xmlwriter.WriteEndElement();

                            xmlwriter.WriteStartElement("td");
                                xmlwriter.WriteString(((ItemObj.ACCharges+ItemObj.RatePerDay)*TotalNoOfDays).ToString());
                            xmlwriter.WriteEndElement();

                        xmlwriter.WriteEndElement();

                        
                        TotalCost+=(ItemObj.ACCharges+ItemObj.RatePerDay)*TotalNoOfDays;
                    }
                    xmlwriter.WriteStartElement("tr");

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteAttributeString("colspan", "4");
                            xmlwriter.WriteAttributeString("class", "txt-align-right");
                            xmlwriter.WriteString("Room Rent");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteString("INR");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteString(TotalCost.ToString());
                        xmlwriter.WriteEndElement();

                    xmlwriter.WriteEndElement();

                    xmlwriter.WriteStartElement("tr");

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteAttributeString("colspan", "4");
                            xmlwriter.WriteAttributeString("class", "txt-align-right");
                            xmlwriter.WriteString("Total");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteString("INR");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteString(TotalCost.ToString());
                        xmlwriter.WriteEndElement();

                    xmlwriter.WriteEndElement();

                    // xmlwriter.WriteStartElement("tr");

                    //     xmlwriter.WriteStartElement("td");
                    //         xmlwriter.WriteAttributeString("colspan", "4");
                    //         xmlwriter.WriteAttributeString("class", "txt-align-right");
                    //         xmlwriter.WriteString("Discount Amount");
                    //     xmlwriter.WriteEndElement();

                    //     xmlwriter.WriteStartElement("td");
                    //         xmlwriter.WriteString("INR");
                    //     xmlwriter.WriteEndElement();

                    //     xmlwriter.WriteStartElement("td");
                    //         xmlwriter.WriteString("0.00");
                    //     xmlwriter.WriteEndElement();

                    // xmlwriter.WriteEndElement();

                    xmlwriter.WriteStartElement("tr");

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteAttributeString("colspan", "4");
                            xmlwriter.WriteAttributeString("class", "txt-align-right");
                            xmlwriter.WriteString("Sub Total");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteString("INR");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteString(TotalCost.ToString());
                        xmlwriter.WriteEndElement();

                    xmlwriter.WriteEndElement();

                    xmlwriter.WriteStartElement("tr");

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteAttributeString("colspan", "4");
                            xmlwriter.WriteAttributeString("class", "txt-align-right");
                            xmlwriter.WriteString("Total Taxes");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteString("INR");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteString("0.00");
                        xmlwriter.WriteEndElement();

                    xmlwriter.WriteEndElement();

                    xmlwriter.WriteStartElement("tr");

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteAttributeString("colspan", "4");
                            xmlwriter.WriteAttributeString("class", "txt-align-right");
                            xmlwriter.WriteString("Advance Amount");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteString("INR");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteString((TotalCost/2).ToString());
                        xmlwriter.WriteEndElement();

                    xmlwriter.WriteEndElement();

                    xmlwriter.WriteStartElement("tr");

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteAttributeString("colspan", "4");
                            xmlwriter.WriteAttributeString("class", "txt-align-right");
                            xmlwriter.WriteString("Total Outstanding Amount");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteString("INR");
                        xmlwriter.WriteEndElement();

                        xmlwriter.WriteStartElement("td");
                            xmlwriter.WriteString((TotalCost/2).ToString());
                        xmlwriter.WriteEndElement();

                    xmlwriter.WriteEndElement();

                xmlwriter.WriteEndElement();
            }
            
            String xmlHeader = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
            String XmlString = builder.ToString().Substring(xmlHeader.Length);

            EmailTemplate = EmailTemplate.Replace("@@AMOUNT@@", Convert.ToString(TotalCost/2));
            EmailTemplate = EmailTemplate.Replace("@@PAYMENTSUMMERY@@", XmlString);
            EmailTemplate=EmailTemplate.Replace("[WEBLINK]",_appSettings.ClientSite);
            EmailTemplate = EmailTemplate.Replace("@@LastFooterText@@", "© " + DateTime.Now.Year.ToString() + " Janani Greens. All rights reserved");
            
            var IsMailSend=CommonMethod.SendMail(_appSettings.UserName,_appSettings.Password, EmailTemplate, formData.Email, "Advance Received",_appSettings.Host,_appSettings.Port,_appSettings.EnableSsl);

        }

        private void SendCheckoutMail(Object Obj)
        {
            BookingDetailsAndStatusModel bookingDetailsAndStatus=JsonConvert.DeserializeObject<BookingDetailsAndStatusModel>(Obj.ToString());
            String contentRootPath = _env.ContentRootPath;

            String path = Path.Combine(contentRootPath , "Content","EmailTemplates","Checkout_Complete.html");
            String EmailTemplate = String.Empty;
            EmailTemplate = CommonMethod.ReadHtmlFile(path);

            EmailTemplate = EmailTemplate.Replace("@@BOOKINGSTATUS@@", "Guest checked out");
            EmailTemplate = EmailTemplate.Replace("@@BOOKINGID@@", bookingDetailsAndStatus.BookingId);
            EmailTemplate = EmailTemplate.Replace("@@USERNAME@@", bookingDetailsAndStatus.CustomerName);
            EmailTemplate = EmailTemplate.Replace("@@FEEDBACKLINK@@", _appSettings.ClientSite+"/feedback-form"+"/"+bookingDetailsAndStatus.BookingId);
            EmailTemplate = EmailTemplate.Replace("@@BOOKINGSUMMERY@@", _appSettings.ClientSite+"/booking-summery"+"/"+bookingDetailsAndStatus.BookingId);

            EmailTemplate=EmailTemplate.Replace("[WEBLINK]",_appSettings.ClientSite);
            EmailTemplate = EmailTemplate.Replace("@@LastFooterText@@", "© " + DateTime.Now.Year.ToString() + " Janani Greens. All rights reserved");

            var IsMailSend=CommonMethod.SendMail(_appSettings.UserName,_appSettings.Password, EmailTemplate, bookingDetailsAndStatus.StatusList, "Happy Checking out",_appSettings.Host,_appSettings.Port,_appSettings.EnableSsl);
        }

        private void SendCheckinMail(Object Obj)
        {
            BookingDetailsAndStatusModel bookingDetailsAndStatus=JsonConvert.DeserializeObject<BookingDetailsAndStatusModel>(Obj.ToString());
            String contentRootPath = _env.ContentRootPath;

            String path = Path.Combine(contentRootPath , "Content","EmailTemplates","Checkedin.html");
            String EmailTemplate = String.Empty;
            EmailTemplate = CommonMethod.ReadHtmlFile(path);

            EmailTemplate = EmailTemplate.Replace("@@BOOKINGSTATUS@@", "You checked in");
            EmailTemplate = EmailTemplate.Replace("@@BOOKINGID@@", bookingDetailsAndStatus.BookingId);
            EmailTemplate = EmailTemplate.Replace("@@USERNAME@@", bookingDetailsAndStatus.CustomerName);
            EmailTemplate=EmailTemplate.Replace("[WEBLINK]",_appSettings.ClientSite);
            EmailTemplate = EmailTemplate.Replace("@@LastFooterText@@", "© " + DateTime.Now.Year.ToString() + " Janani Greens. All rights reserved");

            var IsMailSend=CommonMethod.SendMail(_appSettings.UserName,_appSettings.Password, EmailTemplate, bookingDetailsAndStatus.StatusList, "Welcome to Janani Greens",_appSettings.Host,_appSettings.Port,_appSettings.EnableSsl);
        }

        private void SendCancellationMail(Object Obj)
        {
            BookingDetailsAndStatusModel bookingDetailsAndStatus=JsonConvert.DeserializeObject<BookingDetailsAndStatusModel>(Obj.ToString());
            String contentRootPath = _env.ContentRootPath;

            String path = Path.Combine(contentRootPath , "Content","EmailTemplates","Order_cancellation_response.html");
            String EmailTemplate = String.Empty;
            EmailTemplate = CommonMethod.ReadHtmlFile(path);

            EmailTemplate = EmailTemplate.Replace("@@BOOKINGSTATUS@@", "Booking request cancelled");
            EmailTemplate = EmailTemplate.Replace("@@REASON@@", "has been cancelled as per your request");
            EmailTemplate = EmailTemplate.Replace("@@BOOKINGID@@", bookingDetailsAndStatus.BookingId);
            EmailTemplate = EmailTemplate.Replace("@@USERNAME@@", bookingDetailsAndStatus.CustomerName);
            EmailTemplate = EmailTemplate.Replace("@@LastFooterText@@", "© " + DateTime.Now.Year.ToString() + " Janani Greens. All rights reserved");

            var IsMailSend=CommonMethod.SendMail(_appSettings.UserName,_appSettings.Password, EmailTemplate, bookingDetailsAndStatus.StatusList, "Reservation cancelled",_appSettings.Host,_appSettings.Port,_appSettings.EnableSsl);
        }
// JG@$^2152
        private void SendRequestCancellationMail(Object Obj)
        {
            var bookingCancelDetails=JsonConvert.DeserializeObject<List<CancelBookingRequestViewModel>>(Obj.ToString());
            String contentRootPath = _env.ContentRootPath;

            String path = Path.Combine(contentRootPath , "Content","EmailTemplates","Order_cancellation_response.html");
            String EmailTemplate = String.Empty;
            EmailTemplate = CommonMethod.ReadHtmlFile(path);

            foreach(var m in bookingCancelDetails)
            {
                String tempEmailTemplate=EmailTemplate;
                tempEmailTemplate = tempEmailTemplate.Replace("@@BOOKINGSTATUS@@", "Booking request cancelled");
                tempEmailTemplate = tempEmailTemplate.Replace("@@REASON@@", " for the period "+ m.BookingStartDate.ToString("dd/MM/yyyy hh:mm tt")+ " to "+m.BookingEndDate.ToString("dd/MM/yyyy hh:mm tt") +" has been cancelled as someone else has paid for the same room within your requested period before you");
                tempEmailTemplate = tempEmailTemplate.Replace("@@BOOKINGID@@", m.BookingRequestId);
                tempEmailTemplate = tempEmailTemplate.Replace("@@USERNAME@@", m.Name);
                tempEmailTemplate = tempEmailTemplate.Replace("@@LastFooterText@@", "© " + DateTime.Now.Year.ToString() + " Janani Greens. All rights reserved");

                var IsMailSend=CommonMethod.SendMail(_appSettings.UserName,_appSettings.Password, tempEmailTemplate, m.Email, "Booking request cancelled",_appSettings.Host,_appSettings.Port,_appSettings.EnableSsl);
            }

            
        }
    
        // [HttpGet("[action]/{bookingid}")]
        // public IActionResult GeneratePDf([FromRoute] String bookingid)
        // {
        //     var data=_dm.GetBookingDetails(bookingid,_db);

        //     String AbsolutePath= System.IO.Path.Combine(_env.WebRootPath,"Images/");
        //     String contentRootPath = _env.ContentRootPath;
        //     String Message=String.Empty;
        //     String path = System.IO.Path.Combine(contentRootPath , "Content","EmailTemplates","Invoice.html");
        //     String outputPath= System.IO.Path.Combine(contentRootPath,"Content","PDFs",DateTime.Now.Ticks.ToString()+".pdf");
        //     String outputFolder= System.IO.Path.Combine(contentRootPath,"Content","PDFs");
            
        //     if(!Directory.Exists(outputFolder))
        //     {
        //         Directory.CreateDirectory(outputFolder);
        //     }

        //     var globalSettings = new GlobalSettings
        //     {
        //         ColorMode = ColorMode.Color,
        //         Orientation = Orientation.Portrait,
        //         PaperSize = PaperKind.A4,
        //         Margins = new MarginSettings { Top = 10 },
        //         DocumentTitle = "Invoice",
        //         Out = outputPath
        //     };

        //     var objectSettings = new ObjectSettings
        //     {
        //         PagesCount = true,
        //         HtmlContent = CommonMethod.ReadHtmlFile(path),
        //         WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet =  Path.Combine(contentRootPath,"Content", "assets", "styles.css") },
        //         HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
        //         FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
        //     };

        //     var pdf = new HtmlToPdfDocument()
        //     {
        //         GlobalSettings = globalSettings,
        //         Objects = { objectSettings }
        //     };
        //     _converter.Convert(pdf);

        //     return Ok("Done");
        // }
    
    
    }
}