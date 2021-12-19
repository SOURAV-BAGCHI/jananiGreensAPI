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
    public class BookingController:ControllerBase
    {
        private ApplicationDbContext _db;
        private readonly AppSettings _appSettings;
        private readonly IWebHostEnvironment _env;
        private readonly IDataMethods _dm;
        public BookingController(ApplicationDbContext db, IOptions<AppSettings> appSettings,
        IWebHostEnvironment env,IDataMethods dm)
        {
            _db=db;
            _appSettings=appSettings.Value;
            _env=env;
            _dm=dm;
        }

        private enum Status{
            Cancel,
            Verified,
            PaymentDone,
            Checkedin,
            Checkedout
        }

        // [HttpGet("[action]/{startdate}/{enddate}")]
        // public IActionResult CheckAvailability([FromRoute] String startdate,[FromRoute] String enddate)
        // {
        //     List<RoomDetailsViewModel> RoomDetailsList=new List<RoomDetailsViewModel>();
        //     CultureInfo provider = CultureInfo.InvariantCulture;  
            
        //     DateTime dateTimestartdt = DateTime.ParseExact(startdate, "dd-MM-yyyy HH:mm", provider);
        //     DateTime dateTimeenddt = DateTime.ParseExact(enddate, "dd-MM-yyyy HH:mm", provider);
            
        //     SqlParameter param1 = new SqlParameter("@startDate", dateTimestartdt); 
        //     SqlParameter param2 = new SqlParameter("@endDate", dateTimeenddt);

        //     var roomsAvailable =  _db.Set<RoomModel>().FromSqlRaw("usp_CheckAvailability {0},{1}",param1,param2).ToList();
        //     foreach(var m in roomsAvailable)
        //     {
        //         var tempRoomDetails=new RoomDetailsViewModel()
        //         {
        //             RoomId=m.RoomId,
        //             Name=m.Name,
        //             RatePerDay=m.RatePerDay,
        //             IsACAvailable=m.IsACAvailable,
        //             ACCharges=m.ACCharges,
        //             Discount=m.Discount,
        //             MaxNoOfPersons=Convert.ToInt16(m.MaxNoOfPersons),
        //             Features=JsonConvert.DeserializeObject<String[]>(m.Features),
        //             // Description=m.Description,
        //             NumberOfPersons=Convert.ToInt16(m.MaxNoOfPersons),
        //             ImageList=JsonConvert.DeserializeObject<String[]>(m.ImageList),
        //             ImageNameList=JsonConvert.DeserializeObject<String[]>(m.ImageNameList)

        //         };

        //         for(int i =0; i<tempRoomDetails.ImageList.Length;i++)
        //         {
        //             tempRoomDetails.ImageList[i]=_appSettings.Site+"Images/"+tempRoomDetails.ImageList[i];
        //         }

        //         RoomDetailsList.Add(tempRoomDetails);
        //     }
        //     return Ok(RoomDetailsList);
        // }


        [HttpPost("[action]")]
        public IActionResult CheckAvailability([FromBody] CheckAvailabilityViewModel formData)
        {
            String startdate=formData.StartDate, enddate=formData.EndDate;
            List<RoomDetailsViewModel> RoomDetailsList=new List<RoomDetailsViewModel>();
            CultureInfo provider = CultureInfo.InvariantCulture;  
            
            DateTime dateTimestartdt = DateTime.ParseExact(startdate, "dd-MM-yyyy HH:mm", provider);
            DateTime dateTimeenddt = DateTime.ParseExact(enddate, "dd-MM-yyyy HH:mm", provider);
            
            SqlParameter param1 = new SqlParameter("@startDate", dateTimestartdt); 
            SqlParameter param2 = new SqlParameter("@endDate", dateTimeenddt);

            var roomsAvailable =  _db.Set<RoomModel>().FromSqlRaw("usp_CheckAvailability {0},{1}",param1,param2).ToList();
            
            roomsAvailable=CommonMethodLib.CommonMethod.ModifyRoomDetailsAccordingToMonths(roomsAvailable);
            
            foreach(var m in roomsAvailable)
            {
                var tempRoomDetails=new RoomDetailsViewModel()
                {
                    RoomId=m.RoomId,
                    Name=m.Name,
                    RatePerDay=m.RatePerDay,
                    IsACAvailable=m.IsACAvailable,
                    ACCharges=m.ACCharges,
                    Discount=m.Discount,
                    MaxNoOfPersons=Convert.ToInt16(m.MaxNoOfPersons),
                    Features=JsonConvert.DeserializeObject<String[]>(m.Features),
                    // Description=m.Description,
                    NumberOfPersons=Convert.ToInt16(m.MaxNoOfPersons),
                    ImageList=JsonConvert.DeserializeObject<String[]>(m.ImageList),
                    ImageNameList=JsonConvert.DeserializeObject<String[]>(m.ImageNameList)

                };

                for(int i =0; i<tempRoomDetails.ImageList.Length;i++)
                {
                    tempRoomDetails.ImageList[i]=_appSettings.Site+"Images/"+tempRoomDetails.ImageList[i];
                }

                RoomDetailsList.Add(tempRoomDetails);
            }

            
            return Ok(RoomDetailsList);
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> GenerateBookingIntimation([FromBody] BookingRequestViewModel formData)
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
                var TmpRoomDetails= await _db.Rooms.FindAsync(m.RoomId);
                var TempRoomDetails=new RoomModel()
                {
                    RoomId=TmpRoomDetails.RoomId,
                    Name=TmpRoomDetails.Name,
                    RatePerDay=TmpRoomDetails.RatePerDay,
                    IsACAvailable=TmpRoomDetails.IsACAvailable,
                    ACCharges=TmpRoomDetails.ACCharges,
                    Discount=TmpRoomDetails.Discount,
                    MaxNoOfPersons=TmpRoomDetails.MaxNoOfPersons,
                    Features=TmpRoomDetails.Features,
                    Description=TmpRoomDetails.Description,
                    ImageList=TmpRoomDetails.ImageList,
                    ImageNameList=TmpRoomDetails.ImageNameList
                };
                TempRoomDetails=CommonMethodLib.CommonMethod.ModifyRoomDetailsAccordingToMonths(TempRoomDetails);

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
                IsVerified=false,
                CreateDate=DateTime.Now
            };


            await _db.BookingRequestQueue.AddAsync(newformData);
            await _db.SaveChangesAsync();

            var Obj= JsonConvert.SerializeObject(newformData);
            // var Obj= JsonSerializer.Serialize<BookingRequestQueueModel>(formData);

            ParameterizedThreadStart parameterizedThreadStartSendIntimationToCustomer = new ParameterizedThreadStart(SendIntimationToCustomer);
            Thread Thread1 = new Thread(parameterizedThreadStartSendIntimationToCustomer);
            Thread1.Start(Obj);

            // ParameterizedThreadStart parameterizedThreadStartSendIntimationToAdmin = new ParameterizedThreadStart(SendIntimationToAdmin);
            // Thread Thread2 = new Thread(parameterizedThreadStartSendIntimationToAdmin);
            // Thread2.Start(Obj);

            return Ok(1);
        }
        
        [HttpGet("[action]/{BookingRequestId}")]
        public async Task<IActionResult> GetBookingDetails([FromRoute] String BookingRequestId)
        {
            var BookingDetails=await _db.BookingRequestQueue.FindAsync(BookingRequestId);

            if(BookingDetails==null)
            {
                return BadRequest();
            }
            else
            {
                return Ok(BookingDetails);
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> VerifyBookingDetails([FromBody] BookingRequestVerifyViewModel formData)
        {
            formData.VerificationCode=Int64.Parse(formData.BookingRequestId);
            var BookingDetails=await _db.BookingRequestQueue.FindAsync(formData.BookingRequestId);

            if(BookingDetails == null)
            {
                return BadRequest();
            }
            else
            {
                if(!BookingDetails.IsVerified)
                {
                    if((DateTime.Now-BookingDetails.CreateDate).TotalHours<=_appSettings.VerificationTimeLimitInHrs)
                    {
                        if(BookingDetails.VerificationCode==formData.VerificationCode)
                        {
                            BookingDetails.IsVerified=true;

                            _db.Entry(BookingDetails).State=EntityState.Modified;

                            // await _db.SaveChangesAsync();

                            var Obj= JsonConvert.SerializeObject(BookingDetails);

                            ParameterizedThreadStart parameterizedThreadStartSendVerificationComletedAndPaymentMail = new ParameterizedThreadStart(SendVerificationComletedAndPaymentMail);
                            Thread Thread1 = new Thread(parameterizedThreadStartSendVerificationComletedAndPaymentMail);
                            Thread1.Start(Obj);


                            _dm.InitiateBookingDetailsAndStatus(Obj,_db);
                            
                            return Ok(1);   // 1 means verification done
                        }
                        else
                        {
                            return Ok(-1);  // -1 means incorrect verification code
                        }
                    }
                    else
                    {
                        return Ok(0);   // 0 means verification time exceeded
                    }
                }
                else
                {
                    return Ok(2);   // 2 means already verified
                }
            }
            
        }

        // private void InitiateBookingDetailsAndStatus(Object Obj)
        // {
        //     BookingRequestQueueModel formData=JsonConvert.DeserializeObject<BookingRequestQueueModel>(Obj.ToString());
        //     List<StatusRecordViewModel> SRVMObjList=new List<StatusRecordViewModel>(){
        //         new StatusRecordViewModel(){
        //             Status=1,
        //             StatusDate=DateTime.Now.ToString("dd-MM-yyyy HH:mm")
        //         }
        //     };

        //     var data=new BookingDetailsAndStatusModel()
        //     {
        //         BookingId=formData.BookingRequestId,
        //         Checkin=DateTime.Now,
        //         Checkout=DateTime.Now,
        //         Status=1,
        //         StatusList=JsonConvert.SerializeObject(SRVMObjList),
        //         RoomCheckinCheckoutDetails=String.Empty,
        //         IsProcessComplete=false,
        //         BookingStartDate=formData.BookingStartDate,
        //         CustomerName=formData.Name
        //     };

        //     _db.BookingDetailsAndStatus.Add(data);
        //     _db.SaveChanges();

        //     return;
        // }
        
        [HttpPut("[action]")]
        public async Task<IActionResult> BookingCancellation([FromBody] CancellationRequestViewModel formData)
        {
            var BookingDetails=await _db.BookingDetailsAndStatus.FindAsync(formData.BookingRequestId);

            if(BookingDetails==null)
            {
                return BadRequest(-1);
            }
            else
            {
                if(BookingDetails.Status==(short)Status.PaymentDone)
                {
                    
                    var data=await _db.CancellationRequests.FindAsync(formData.BookingRequestId);

                    if(data==null)
                    {
                        var CancellationRequst=new CancellationRequestModel(){
                            BookingRequestId=formData.BookingRequestId,
                            CustomerName=BookingDetails.CustomerName,
                            RequestDateTime=DateTime.Now,
                            BookingStartDate=BookingDetails.BookingStartDate,
                            Reason=formData.Reason,
                            CancellationAccepted=false
                        };

                        await _db.CancellationRequests.AddAsync(CancellationRequst);
                        await _db.SaveChangesAsync();

                        return Ok(1);
                    }
                    else
                    {
                        if(data.CancellationAccepted)
                        {
                            return BadRequest(1);
                        } 
                        else
                        {
                            return BadRequest(0);
                        }
                    }
                    
                }
                else
                {
                    return BadRequest(2);
                }
            }
        }

        [HttpGet("[action]/{bookingid}")]
        public IActionResult GetBookingSummery([FromRoute] String bookingid)
        {
            var data=_dm.GetBookingDetails(bookingid,_db);

            if(data ==null)
            {
                return BadRequest();
            }
            else
            {
                return Ok(
                    new BookingDetailsClientViewModel()
                    {
                        BookingId=data.BookingId,
                        Name=data.Name,
                        Phone=data.Phone,
                        Email=data.Email,
                        CreateDate=data.CreateDate,
                        BookingStartDate=data.BookingStartDate,
                        BookingEndDate=data.BookingEndDate,
                        StatusList=((data.Status==-1)? 
                        
                            (new List<StatusRecordViewModel>()
                                {
                                    new StatusRecordViewModel(){
                                        Status=data.Status,
                                        StatusDate=data.CreateDate.ToString("dd-MM-yyyy HH:mm")
                                    }
                                }
                            ):
                            (
                                JsonConvert.DeserializeObject<List<StatusRecordViewModel>>(data.StatusList)
                            )

                        ),
                        Status=data.Status,
                        Checkin=data.Checkin,
                        Checkout=data.Checkout,
                        RoomOrderDetails=JsonConvert.DeserializeObject<List<RoomOrderBasicViewModel>>(data.RoomOrderDetails),
                        NumberOfDays=Convert.ToInt32((data.BookingEndDate-data.BookingStartDate).TotalDays)
                    }
                );
            }

        }

        private void SendIntimationToCustomer(Object Obj)
        {
            BookingRequestQueueModel formData=JsonConvert.DeserializeObject<BookingRequestQueueModel>(Obj.ToString());
            List<RoomOrderBasicViewModel> ROBVMObjList=JsonConvert.DeserializeObject<List<RoomOrderBasicViewModel>>(formData.RoomOrderDetails);

            String AbsolutePath=Path.Combine(_env.WebRootPath,"Images/");

            String contentRootPath = _env.ContentRootPath;
            var TotalNoOfDays=Convert.ToInt32((formData.BookingEndDate-formData.BookingStartDate).TotalDays);

            String path = Path.Combine(contentRootPath , "Content","EmailTemplates","Order_Details_Summery_Customer.html");
            String EmailTemplate = String.Empty;
            EmailTemplate = CommonMethod.ReadHtmlFile(path);

            EmailTemplate = EmailTemplate.Replace("@@BOOKINGSTATUS@@", "Booking Under Process");
            EmailTemplate = EmailTemplate.Replace("@@BOOKINGID@@", formData.BookingRequestId);
            EmailTemplate = EmailTemplate.Replace("@@USERNAME@@", formData.Name);
            EmailTemplate = EmailTemplate.Replace("@@ESTIMATEDDELIVERYDATE@@", formData.BookingStartDate.ToString()+" - "+formData.BookingEndDate.ToString());
            EmailTemplate = EmailTemplate.Replace("@@YOURBOOKINGDONEON@@", DateTime.Now.ToString());
            EmailTemplate = EmailTemplate.Replace("@@PHONE@@", formData.Phone);
            EmailTemplate = EmailTemplate.Replace("@@EMAIL@@", formData.Email);
            EmailTemplate = EmailTemplate.Replace("@@BOOKINGID@@", formData.BookingRequestId);
            EmailTemplate = EmailTemplate.Replace("@@ORDERTRACKPAGE@@",_appSettings.ClientSite+"verify-booking/"+formData.BookingRequestId);

            Double TotalCost=0.0;
            var builder = new StringBuilder();
            using (var xmlwriter = XmlWriter.Create(builder))
            {
                xmlwriter.WriteStartElement("table");
                xmlwriter.WriteAttributeString("class", "table-orderdetails");

                foreach (var ItemObj in ROBVMObjList)
                {
                    xmlwriter.WriteStartElement("tr");

                    xmlwriter.WriteStartElement("td");
                    xmlwriter.WriteAttributeString("width", "30");
                       
                        xmlwriter.WriteStartElement("img");
                        xmlwriter.WriteAttributeString("src", _appSettings.Site+"Images/" + ItemObj.Image);
                        xmlwriter.WriteAttributeString("class", "product-image");
                        xmlwriter.WriteEndElement();


                    xmlwriter.WriteEndElement();   // ending td

                    xmlwriter.WriteStartElement("td");
                        xmlwriter.WriteAttributeString("width", "40");
                        xmlwriter.WriteStartElement("label");
                        xmlwriter.WriteAttributeString("class", "disp_block");
                        xmlwriter.WriteString(ItemObj.Name);

                    xmlwriter.WriteEndElement();        //end of label

                    xmlwriter.WriteStartElement("div");
                    xmlwriter.WriteAttributeString("class", "disp_block");

                    if(ItemObj.IsAcAvailed)
                    {
                        xmlwriter.WriteElementString("label", "AC availed:" + "Yes");
                        xmlwriter.WriteEndElement();        //end of div
                    }   
                    else
                    {
                        xmlwriter.WriteElementString("label", "AC available:" + "No");
                        xmlwriter.WriteEndElement();        //end of div
                    }
                    
                    
                    
                    xmlwriter.WriteElementString("label", "No. of person :" + ItemObj.NumberOfPerson.ToString());

                    xmlwriter.WriteEndElement();        // second td

                    xmlwriter.WriteStartElement("td");
                    xmlwriter.WriteAttributeString("width", "30");
                        xmlwriter.WriteElementString("label", "Rs." + (ItemObj.ACCharges+ItemObj.RatePerDay).ToString() +" x "+TotalNoOfDays.ToString());
                    xmlwriter.WriteEndElement();        //third td

                    xmlwriter.WriteEndElement();   // ending tr

                    TotalCost+=(ItemObj.ACCharges+ItemObj.RatePerDay)*TotalNoOfDays;
                }

                xmlwriter.WriteEndElement();
            }
            String xmlHeader = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
            String XmlString = builder.ToString().Substring(xmlHeader.Length);

            EmailTemplate = EmailTemplate.Replace("@@ORDERPRODUCTDETAILS@@", XmlString);

            EmailTemplate = EmailTemplate.Replace("@@ITEMSUBTOTALCOST@@", Convert.ToString(TotalCost));
            // EmailTemplate = EmailTemplate.Replace("@@APPLIEDPROMOTIONNAME@@", "None");
            // EmailTemplate = EmailTemplate.Replace("@@PROMOTIONAPPLIED@@", Convert.ToString(0.0));
            EmailTemplate = EmailTemplate.Replace("@@ORDERTOTAL@@", Convert.ToString(TotalCost));

            EmailTemplate=EmailTemplate.Replace("[WEBLINK]",_appSettings.ClientSite);
            EmailTemplate = EmailTemplate.Replace("@@LastFooterText@@", "© " + DateTime.Now.Year.ToString() + " Janani Greens. All rights reserved");


            var IsMailSend=CommonMethod.SendMail(_appSettings.UserName,_appSettings.Password, EmailTemplate, formData.Email, "Booking Request",_appSettings.Host,_appSettings.Port,_appSettings.EnableSsl);
        }
        
        private void SendVerificationComletedAndPaymentMail(Object Obj)
        {
            BookingRequestQueueModel formData=JsonConvert.DeserializeObject<BookingRequestQueueModel>(Obj.ToString());
            List<RoomOrderBasicViewModel> ROBVMObjList=JsonConvert.DeserializeObject<List<RoomOrderBasicViewModel>>(formData.RoomOrderDetails);

            String AbsolutePath=Path.Combine(_env.WebRootPath,"Images/");
            String contentRootPath = _env.ContentRootPath;

            String path = Path.Combine(contentRootPath , "Content","EmailTemplates","Verification_confirmation_payment_info.html");
            String EmailTemplate = String.Empty;
            EmailTemplate = CommonMethod.ReadHtmlFile(path);

            EmailTemplate = EmailTemplate.Replace("@@BOOKINGSTATUS@@", "Booking requested");
            EmailTemplate = EmailTemplate.Replace("@@BOOKINGID@@", formData.BookingRequestId);
            EmailTemplate = EmailTemplate.Replace("@@USERNAME@@", formData.Name);
            EmailTemplate = EmailTemplate.Replace("@@ESTIMATEDDELIVERYDATE@@", formData.BookingStartDate.ToString()+" - "+formData.BookingEndDate.ToString());
            EmailTemplate = EmailTemplate.Replace("@@YOURBOOKINGDONEON@@", DateTime.Now.ToString());
            EmailTemplate = EmailTemplate.Replace("@@PHONE@@", formData.Phone);
            EmailTemplate = EmailTemplate.Replace("@@EMAIL@@", formData.Email);
            
            
            Double TotalCost=0.0;
            var TotalNoOfDays=Convert.ToInt32((formData.BookingEndDate-formData.BookingStartDate).TotalDays);

            foreach (var ItemObj in ROBVMObjList)
            {
                TotalCost+=(ItemObj.ACCharges+ItemObj.RatePerDay)*TotalNoOfDays;
            }

            EmailTemplate = EmailTemplate.Replace("@@TOTALAMOUNT@@", Convert.ToString(TotalCost));
            EmailTemplate = EmailTemplate.Replace("@@ADVANCEAMOUNT@@", Convert.ToString(TotalCost/2));
            EmailTemplate=EmailTemplate.Replace("[WEBLINK]",_appSettings.ClientSite);
            EmailTemplate = EmailTemplate.Replace("@@LastFooterText@@", "© " + DateTime.Now.Year.ToString() + " Janani Greens. All rights reserved");
            var IsMailSend=CommonMethod.SendMail(_appSettings.UserName,_appSettings.Password, EmailTemplate, formData.Email, "Please pay the advance",_appSettings.Host,_appSettings.Port,_appSettings.EnableSsl);
        }
            
    }
}