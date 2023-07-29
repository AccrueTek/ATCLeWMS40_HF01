using QITS_IntegrationWebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace QITS_IntegrationWebApi.Controllers
{
    public class CommonController : ApiController
    {
        //WMSDataContext dbcontext = new WMSDataContext();
        WmsService.Service1 wmsServiceClient = new WmsService.Service1();
        WMSDatabaseDataContext newDBContext = new WMSDatabaseDataContext();

        CAEeWMSDatabaseDataContext caeDBContext = new CAEeWMSDatabaseDataContext();


        HttpError err = new HttpError();
        string errmsg = "";

        [HttpGet]
        [Route("api/WebApi/Login")] // Completed - Web Service
        public IHttpActionResult Login(string UserCode, string Password)
        {
            string UserName = "";
            try
            {
                UserName = wmsServiceClient.Get_WMS_login(UserCode, Password);
                if (string.IsNullOrEmpty(UserName))
                    return Content(System.Net.HttpStatusCode.NotFound, "User not found");
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(UserName);
        }

        [HttpGet]
        [Route("api/WebApi/Login")] // Completed - Web Service
        public IHttpActionResult Login(string UserCode, string Password, string deviceID)
        {
            string UserName = "";
            try
            {
                UserName = wmsServiceClient.Get_WMS_login(UserCode, Password);
                if(string.IsNullOrEmpty(UserName))
                    return Content(System.Net.HttpStatusCode.NotFound, "User not found");
                string errorMessage = string.Empty;
                if(ValidateLogin(UserCode, deviceID, out errorMessage))
                {
                    var loginInfoID = InsertLoginTable(UserName, UserCode, deviceID);
                    return Ok(string.Format("{0},{1}", UserName, loginInfoID));
                }
                return Content(System.Net.HttpStatusCode.Forbidden, $"Duplicate login - {errorMessage}");


            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
        }

        [HttpGet]
        [Route("api/WebApi/Logout")] // Completed - Web Service
        public IHttpActionResult Logout(int lognInfoId)
        {
            try
            {
                UpdateLoginTable(lognInfoId);
                return Ok("Logout success");
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
        }

        [HttpGet]
        [Route("api/WebApi/LogoutUser")] // Completed - Web Service
        public IHttpActionResult LogoutUser(string userName)
        {
            try
            {
                var loginInfo = caeDBContext.GetTable<LoginInfo>().Where(x => x.UserName.Equals(userName) && x.Status == true && x.IsDeleted == false);
                loginInfo.ToList().ForEach(x =>
                {
                    UpdateLoginTable(x.ID);
                });
                
                return Ok("Logout success");
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
        }


        private int InsertLoginTable(string userId, string userName, string deviceId)
        {
            LoginInfo loginInfo = new LoginInfo()
            {
                UserID = userId, UserName = userName, LoginTime = DateTime.Now, LogoutTime = null, Status = true, IsDeleted = false, DeviceID = deviceId
            };
            caeDBContext.GetTable<LoginInfo>().InsertOnSubmit(loginInfo);
            caeDBContext.SubmitChanges();
            return loginInfo.ID;
        }

        private LoginInfo GetMatchingLoginInfo(string userId, string userName, string deviceId)
        {
            return caeDBContext.GetTable<LoginInfo>().Where(x => x.DeviceID.Equals(deviceId) && x.UserID.Equals(userId) && !x.LoginTime.HasValue).LastOrDefault();
        }

        private void UpdateLoginTable(int loginInfoId)
        {
            var loginInfo = caeDBContext.GetTable<LoginInfo>().FirstOrDefault(x => x.ID == loginInfoId);
            loginInfo.IsDeleted = true;
            loginInfo.Status = false;
            loginInfo.LogoutTime = DateTime.Now;
            caeDBContext.SubmitChanges();
        }

        private bool ValidateLogin(string userName, string deviceId, out string errorMessage)
        {
            var loginTable = caeDBContext.GetTable<LoginInfo>();
            errorMessage = string.Empty;
            if (loginTable.Any() && loginTable.Any(x=> x.UserName.Equals(userName)))
            {
                var lastRecord = loginTable.Where(x => x.UserName.Equals(userName) && x.Status == true && x.IsDeleted == false);
                if (lastRecord == null || lastRecord.ToList().Count == 0)
                    return true;
                else
                {                    
                    var lastLoginInfo = lastRecord.ToList().Last();
                    if (!string.IsNullOrEmpty(deviceId) && lastLoginInfo.DeviceID.Equals(deviceId))
                    {
                        UpdateLoginTable(lastLoginInfo.ID);
                        return true;
                    }
                    errorMessage = $"Last login is from Device : {lastLoginInfo.DeviceID}. Please logout from that the device.";
                    return false;
                }
                    
            }
            return true;
        }

        [HttpGet]
        [Route("api/WebApi/GetLoginInfo")] // Completed - Web Service
        public IHttpActionResult GetLoginInfo()
        {
            List<LoginInfo> result = new List<LoginInfo>();
            try
            {
                result = caeDBContext.GetTable<LoginInfo>().ToList();                
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(result);
        }


        [HttpGet]
        [Route("api/WebApi/GetWMSLicense")] // Completed - Web Service
        public IHttpActionResult GetWMSLicense()
        {
            string UserName = "";
            try
            {

                UserName = wmsServiceClient.Get_WMS_licence();
                if (string.IsNullOrEmpty(UserName))
                    return Content(System.Net.HttpStatusCode.NotFound, "User not found");
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(UserName);
        }

        [HttpGet]
        [Route("api/WebApi/GetOpenPOList")] // Completed - Database
        public IHttpActionResult GetOpenPOList()
        {
            List<OpenPOList> POList = new List<OpenPOList>();            
            try
            {
                var serviceResult = wmsServiceClient.GetOpenPOList();
                if(serviceResult.Any())
                {
                    foreach(var purchaseOrder in serviceResult)
                    {
                        OpenPOList OPL = new OpenPOList();
                        OPL.PODocEntry = purchaseOrder.GBU_ID;
                        OPL.PONum = int.Parse(purchaseOrder.GBU_BoxUID);
                        OPL.SupplierCode = purchaseOrder.GBcode;
                        OPL.SupplierName = purchaseOrder.GBname;
                        if (purchaseOrder.GBU_UsrID == null) { OPL.SupplierRef = ""; }
                        else { OPL.SupplierRef = purchaseOrder.GBU_UsrID; }
                        OPL.CustomerCode = purchaseOrder.GBU_IsDeleted;
                        OPL.CustomerName = purchaseOrder.GBU_BoxST;
                        POList.Add(OPL);
                    }


                    //var PList = (from p in newDBContext.OPORs where p.DocStatus == 'O' select new { p.DocEntry, p.DocNum, p.CardCode, p.CardName, p.NumAtCard }).ToList();
                    //if (PList.Count > 0)
                    //{
                    //    foreach (var PO in PList)
                    //    {
                    //        var Bsentry = (from r in newDBContext.POR1s where r.DocEntry == PO.DocEntry select new { r.BaseEntry, r.BaseType }).Distinct().ToList();
                    //        string custcode = string.Empty;
                    //        string custname = string.Empty;
                    //        if (Bsentry.Count > 0)
                    //        {
                    //            if (Bsentry[0].BaseType == 17)
                    //            {
                    //                var custcd = (from o in newDBContext.ORDRs where o.DocEntry == Bsentry[0].BaseEntry select new { o.CardCode, o.CardName }).ToList();
                    //                if (custcd.Count > 0)
                    //                {
                    //                    custcode = custcd[0].CardCode;
                    //                    custname = custcd[0].CardName;
                    //                }
                    //            }
                    //        }

                    //        OpenPOList OPL = new OpenPOList();
                    //        OPL.PODocEntry = PO.DocEntry;
                    //        OPL.PONum = PO.DocNum;
                    //        OPL.SupplierCode = PO.CardCode;
                    //        OPL.SupplierName = PO.CardName;
                    //        if (PO.NumAtCard == null) { OPL.SupplierRef = ""; }
                    //        else { OPL.SupplierRef = PO.NumAtCard; }
                    //        OPL.CustomerCode = custcode;
                    //        OPL.CustomerName = custname;
                    //        POList.Add(OPL);
                    //    }
                }
                else
                    return Content(System.Net.HttpStatusCode.NotFound, "No open list found");
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(POList);
        }

        [HttpGet]
        [Route("api/WebApi/GetPODetails")] // Completed Service (new service does not return price value, as it is not required for mobie app, sending 0.
        public IHttpActionResult GetPODetails(int PONumber)
        {
            List<PODetails> PODtls = new List<PODetails>();
            try
            {
                var serviceResult = wmsServiceClient.Get_purchase_docnumber(PONumber);
                if(serviceResult.Any())
                {
                    foreach(var poDetail in serviceResult)
                    {
                        PODetails OPD = new PODetails();
                        OPD.ItemCode = poDetail.Itemcode;
                        OPD.Description = poDetail.Itemdiscription;                        
                        if (poDetail.Itemmngbysernum == "B") 
                        { 
                            OPD.ItemType = 'B'; 
                        }
                        else if (poDetail.Itemmngbysernum == "S") 
                        { 
                            OPD.ItemType = 'S'; 
                        }
                        else { OPD.ItemType = 'N'; }
                        
                        OPD.Linenum = poDetail.Linenumber;
                        OPD.OpenQty = (int)poDetail.Quantity;
                        OPD.Price = default(decimal);// (decimal)PO.price; //Price column not found in new Database
                        if (poDetail.SoDocentry != default) { OPD.SoDocEntry = poDetail.SoDocentry; }
                        if (poDetail.SoDocNum != default) { OPD.SoDocNum = poDetail.SoDocNum; }
                        OPD.WHS = poDetail.Whscode;
                        PODtls.Add(OPD);
                    }
                }
                else
                    return Content(System.Net.HttpStatusCode.NotFound, "No PO Details found");

                //var POD = newDBContext.USP_WMS_GetDataByPONumber(PONumber).ToList();
                //if (POD.Count > 0)
                //{
                //    foreach (var PO in POD)
                //    {
                //        PODetails OPD = new PODetails();
                //        OPD.ItemCode = PO.ItemCode;
                //        OPD.Description = PO.Description;
                //        if (PO.ManBtchNum == 'Y') { OPD.ItemType = 'B'; }
                //        else if (PO.ManSerNum == 'Y') { OPD.ItemType = 'S'; }
                //        else { OPD.ItemType = 'N'; }
                //        OPD.Linenum = PO.LineNum;
                //        OPD.OpenQty = (int)PO.OpenQty;
                //        OPD.Price = default(decimal);// (decimal)PO.price; //Price column not found in new Database
                //        if (PO.SoDocEntry != null) { OPD.SoDocEntry = (int)PO.SoDocEntry; }
                //        if (PO.SODocNum != null) { OPD.SoDocNum = (int)PO.SODocNum; }
                //        OPD.WHS = PO.WhsCode;
                //        PODtls.Add(OPD);
                //    }
                //}
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(PODtls);
        }

        [HttpGet]
        [Route("api/WebApi/CheckSerial")] //Completed Service
        public IHttpActionResult CheckSerial(string itemcode, string srno)
        {
            int i = 0;
            try
            {
                i = wmsServiceClient.CheckSerialInStock(itemcode, srno);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(i);
        }

        [HttpGet]
        [Route("api/WebApi/GetAllWHS")] // completed Service
        public IHttpActionResult GetAllWHS()
        {
            string[] WHS;
            try
            {
                WHS = wmsServiceClient.GetAllWHS();
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(WHS);
        }

        [HttpGet]
        [Route("api/WebApi/CheckBin")] // completed Service
        public IHttpActionResult CheckBin(string bin, string whs)
        {
            int i = 0; ;
            try
            {
                i = wmsServiceClient.CheckBin(bin, whs);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(i);
        }

        [HttpGet]
        [Route("api/WebApi/GetAllBox")] // completed Service
        public IHttpActionResult GetAllBox()
        {
            List<Box> BoxList = new List<Box>();
            
            
            try
            {   
                var boxList = wmsServiceClient.GetAllBox();
                if (boxList != null && boxList.ToList().Count > 0)
                {
                    foreach (var box in boxList)
                    {
                        Box Boxl = new Box();
                        Boxl.BoxID = box.GBU_ID; // Box.U_ID
                        Boxl.BoxUID = box.GBU_BoxUID; //Box.U_BoxUID;
                        Boxl.BoxStatus = Char.Parse(box.GBU_BoxST);// (Char)Box.U_BoxST;
                        BoxList.Add(Boxl);
                    }
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(BoxList);
        }

        [HttpGet]
        [Route("api/WebApi/GetAllPallet")] // completed Service
        public IHttpActionResult GetAllPallet()
        {
            List<Pallet> PalList = new List<Pallet>();
            try
            {
                var palletList = wmsServiceClient.GetAllPallet();
                if (palletList!= null && palletList.ToList().Count > 0)
                {
                    foreach (var pallet in palletList)
                    {
                        Pallet Pall = new Pallet();
                        Pall.PalletID = pallet.GBU_ID; //Pal.U_ID;
                        Pall.PalletUID = pallet.GBU_BoxUID;// Pal.U_PalletUID;
                        Pall.PalletStatus = Char.Parse(pallet.GBU_BoxST);// (Char)Pal.U_PallST;
                        PalList.Add(Pall);
                    }
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(PalList);
        }

        [HttpGet]
        [Route("api/WebApi/DeleteBox")] // Completed Service
        public IHttpActionResult DeleteBox(int BoxID)
        {
            string Msg = string.Empty;
            try
            {
                Msg = wmsServiceClient.DeleteBox(BoxID);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(Msg);
        }

        [HttpGet]
        [Route("api/WebApi/DeletePallet")] // Completed Service
        public IHttpActionResult DeletePallet(int PalletID)
        {
            string Msg = "Success";
            try
            {
                Msg = wmsServiceClient.DeletePallet(PalletID);


                //var boxCount = newDBContext._WMS35_BOXLs.Where(box => box.U_PALLID == PalletID).ToList();
                //if(boxCount == null || !boxCount.Any())
                //{
                //    var PH = newDBContext._WMS35_PALLs.Where(ph => ph.U_ID == PalletID).ToList();
                //    if (PH.Count > 0)
                //    {
                //        newDBContext._WMS35_PALLs.DeleteAllOnSubmit(PH);
                //        newDBContext.SubmitChanges();
                //    }
                //}
                //else
                //{
                //    Msg = "Cannot delete the Pallet as it is already mapped to Box.";
                //}
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(Msg);
        }

        [HttpGet] 
        [Route("api/WebApi/CreateBox")] // completed Service
        public IHttpActionResult CreateBox(string Box, int count, string User)
        {
            string[] Boxes;
            try
            {
                Boxes = wmsServiceClient.CreateBox(Box, count, User);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(Boxes);
        }

        [HttpGet]
        [Route("api/WebApi/GetBoxDetails")]  // Completed service
        public IHttpActionResult GetBoxDetails(int BoxId)
        {
            List<BoxDetails> Boxdtls = new List<BoxDetails>();
            try
            {
                var serviceResult = wmsServiceClient.GetBoxDetailByID(BoxId);

                if(serviceResult.Any())
                {
                    foreach(var box in serviceResult)
                    {
                        BoxDetails BDTL = new BoxDetails();
                        BDTL.SoNum = (int)box.GLU_SoNum; //GLU_SoNum
                        BDTL.PickId = box.GLU_SoDocEntry;
                        BDTL.ItemCode = box.GLU_ItemCode; //GLU_ItemCode
                        BDTL.Description = box.GLname;
                        BDTL.Batch = box.GLU_BatchNo; // GLU_BatchNo   
                        BDTL.Serial = box.GLU_SerialNo; //GLU_SerialNo     
                        BDTL.Bin = box.GLU_BinID; //GLU_BinID
                        BDTL.Qty = box.GLU_Qty; //GLU_Qty
                        Boxdtls.Add(BDTL);

                    }
                }
                else
                    return Content(System.Net.HttpStatusCode.NotFound, "No box details found");

                //var BoxDList = newDBContext.USP_WMS35_GetDataByBox(BoxId).ToList();

                //if (BoxDList.Count > 0)
                //{
                //    foreach (var BD in BoxDList)
                //    {
                //        BoxDetails BDTL = new BoxDetails();
                //        BDTL.SoNum = (int)BD.U_SoNum; //GLU_SoNum
                //        BDTL.PickId = BD.U_Pickid;
                //        BDTL.ItemCode = BD.U_ItemCode; //GLU_ItemCode
                //        BDTL.Description = BD.U_ItemDesc;
                //        BDTL.Batch = BD.U_BatchNo; // GLU_BatchNo   
                //        BDTL.Serial = BD.U_SerialNo; //GLU_SerialNo     
                //        BDTL.Bin = BD.Bin; //GLU_BinID
                //        BDTL.Qty = (int)BD.Qty; //GLU_Qty
                //        Boxdtls.Add(BDTL);
                //    }
                //}
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(Boxdtls);
        }

        [HttpGet]
        [Route("api/WebApi/CreatePallet")] // completed service
        public IHttpActionResult CreatePallet(string Pallet, string User)
        {
            int t = 0;
            try
            {
                t = wmsServiceClient.CreatePallet(Pallet, User);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(t);
        }

        [HttpGet]
        [Route("api/WebApi/BoxStatus")] // completed Service
        public IHttpActionResult BoxStatus(int BoxID, char Status)
        {
            int t = 1;
            try
            {
                t = wmsServiceClient.BoxStatus(BoxID, Status.ToString());
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(t);
        }

        [HttpGet]
        [Route("api/WebApi/CheckItem")] // completed Service
        public IHttpActionResult CheckItem(string ItemCode)
        {
            string Itm = "";
            try
            {
                Itm = wmsServiceClient.Check_itemcode(ItemCode);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(Itm);
        }

        [HttpGet]
        [Route("api/WebApi/CheckBatch")] // Completed Database - Used in Stock transfer, Goods Receipt and Goods Issue
        public IHttpActionResult CheckBatch(string ItemCode, string Batch)
        {
            int Qty;
            try
            {

                Qty = wmsServiceClient.CheckBatch(ItemCode, Batch);
                //var q = (from o in newDBContext.OBTNs 
                //         join b in newDBContext.OBTQs 
                //         on o.ItemCode equals b.ItemCode
                //         where o.ItemCode == ItemCode & o.DistNumber == Batch & o.AbsEntry == b.MdAbsEntry select b.Quantity).ToList().FirstOrDefault();
                //if (q != null && q > 0) 
                //{ 
                //    Qty = (int)q; 
                //}
                //else 
                //{ 
                //    Qty = 0; 
                //}
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(Qty);
        }

        [HttpGet]
        [Route("api/WebApi/CheckBatchInBin")] // completed service- Used in Stock transfer, Goods Receipt and Goods Issue
        public IHttpActionResult CheckBatchInBin(string ItemCode, string Batch, string WHS, string Bin)
        {
            int Qty;
            try
            {
                Qty = wmsServiceClient.CheckBatchInBin(ItemCode, Batch, WHS, Bin);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(Qty);
        }

        [HttpGet]
        [Route("api/WebApi/CheckSerialInBin")] // Completed service- Used in Stock transfer, Goods Receipt and Goods Issue
        public IHttpActionResult CheckSerialInBin(string ItemCode, string Serial, string WHS, string Bin)
        {
            int Qty;
            try
            {
                Qty = wmsServiceClient.CheckSerialInBin(ItemCode, Serial, WHS, Bin);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(Qty);
        }

        [HttpGet]
        [Route("api/WebApi/Get_PickLists")] // Completed service
        public IHttpActionResult Get_PickLists(string Status)
        {
            List<PickModel> PL = new List<PickModel>();
            try
            {
                var pickList = wmsServiceClient.Get_PickLists(Status);
                if(pickList != null && pickList.Any())
                {
                    foreach(var pick in pickList)
                    {
                        PL.Add(new PickModel()
                        {
                            PickId = pick.Pickid,
                            Remarks = pick.Remarks
                        });
                    }
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.NotFound, errmsg);
            }
            return Ok(PL);
        }

        [HttpGet]
        [Route("api/WebApi/Get_PickListDetails")]  // Complted database
        public IHttpActionResult Get_PickListDetails(int absentry) // New logic added to get the Pick qty
        {
            List<PickListDetails> PLD = new List<PickListDetails>();
            try
            {
                var serviceResult = wmsServiceClient.Get_PickListDetails(absentry);

                if(serviceResult.Any())
                {
                    foreach (var P in serviceResult)
                    {
                        PickListDetails PLDtls = new PickListDetails();
                        PLDtls.ItemCode = P.Itemcode;
                        PLDtls.Description = P.Itemdiscription;
                        if (P.Itemmngbysernum == "S") { PLDtls.ItemType = 'S'; }
                        else if (P.Itemmngbysernum == "B") { PLDtls.ItemType = 'B'; }
                        else { PLDtls.ItemType = 'N'; }
                        PLDtls.SoDocEntry = (int)P.Docentry;
                        PLDtls.SoNumber = P.SoDocNum;
                        PLDtls.OrderLine = (int)P.OrderLine;
                        PLDtls.Pickentry = P.Linenumber;
                        PLDtls.PickQty = (int)P.Quantity;
                        PLDtls.RelQty = (int)P.Releaseqty;
                        PLDtls.WHSCode = P.Whscode;
                        PLDtls.CustomerCode = P.CustCode;
                        PLDtls.CustomerName = P.CustName;
                        PLDtls.PONumber = P.SoDocentry.ToString();
                        PLDtls.SOOpenQuantity = (int)P.Onhanditemqty;
                        PLDtls.Bin = P.Binabsentry;
                        PLD.Add(PLDtls);
                    }
                }
                else
                    return Content(System.Net.HttpStatusCode.NotFound, $"No Pick list details found for AbsEntry : {absentry}");                
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(PLD);
        }

        [HttpGet]
        [Route("api/WebApi/Get_PackListDetails")]  // Completed database
        public IHttpActionResult Get_PackListDetails(int absentry)
        {
            List<PickListDetails> PLD = new List<PickListDetails>();
            try
            {
                var serviceResult = wmsServiceClient.Get_PackListDetails(absentry);
                if(serviceResult.Any())
                {
                    foreach (var P in serviceResult)
                    {   
                        PickListDetails PLDtls = new PickListDetails();
                        PLDtls.ItemCode = P.Itemcode;
                        PLDtls.Description = P.Itemdiscription;
                        if (P.Itemmngbysernum == "S") { PLDtls.ItemType = 'S'; }
                        else if (P.Itemmngbysernum == "B") { PLDtls.ItemType = 'B'; }
                        else { PLDtls.ItemType = 'N'; }
                        PLDtls.SoDocEntry = (int)P.Docentry;
                        PLDtls.SoNumber = P.SoDocNum;
                        PLDtls.OrderLine = (int)P.OrderLine;
                        PLDtls.Pickentry = P.Linenumber;
                        PLDtls.PickQty = (int)P.Quantity;                        
                        PLDtls.RelQty = (int)P.Releaseqty;
                        PLDtls.WHSCode = P.Whscode;
                        PLDtls.CustomerCode = P.CustCode;
                        PLDtls.CustomerName = P.CustName;
                        PLDtls.PONumber = P.SoDocentry.ToString();
                        PLDtls.SOOpenQuantity = (int)P.Onhanditemqty;
                        PLDtls.Bin = P.Binabsentry;
                        PLD.Add(PLDtls);
                    }
                }
                else
                    return Content(System.Net.HttpStatusCode.NotFound, $"No Pack list details found for AbsEntry : {absentry}");
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(PLD);
        }

        [HttpGet]
        [Route("api/WebApi/Unpack")] // Completed service
        public IHttpActionResult Unpack(int PickID)
        {
            string result = "Success";
            try
            {
                var B = wmsServiceClient.Unpack(PickID); //(from o in newDBContext._WMS35_BOXLs where o.U_PickID == PickID select o).ToList();
                if (!B.ToString().Equals("success"))                
                {
                    result = "No matching items found";
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/Unpick")] // Vinay will restructure this API- Vinay updated the latest API in 3.5 adn the same is updated here.
        public IHttpActionResult Unpick(int PickID)
        {
            string result = "Success";
            try
            {
                result = wmsServiceClient.Unpick(PickID);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/ClosePickList")]  // Vinay will restructure this API- Vinay updated the latest API in 3.5 adn the same is updated here.
        public IHttpActionResult ClosePickList(int PickID)
        {
            string result = "Success";
            try
            {
                result = wmsServiceClient.Close_PL(PickID);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/CheckPallet")] // Completed Service
        public IHttpActionResult CheckPallet(string Pallet)
        {
            int i = 0;
            try
            {
                i = wmsServiceClient.CheckPallet(Pallet);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(i);
        }

        [HttpGet]
        [Route("api/WebApi/CheckBox")] // Completed Service
        public IHttpActionResult CheckBox(string Box)
        {
            int i = 0;
            try
            {
                i = wmsServiceClient.CheckBox(Box);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(i);
        }

        [HttpGet]
        [Route("api/WebApi/CheckPickedBatch")] // Completed Database
        public IHttpActionResult CheckPickedBatch(int PickID, int Pickentry, string Itemcode, string Batch, int Qty)
        {
            int i = 0;
            try
            {
                i = wmsServiceClient.CheckPickedBatch(PickID, Pickentry, Itemcode, Batch, Qty);


                //var BAbs = (from B in newDBContext.OBTNs where B.DistNumber == Batch & B.ItemCode == Itemcode  select new { B.AbsEntry }).ToList();
                //if (BAbs.Count > 0)
                //{
                //    var P2 = (from P in newDBContext.PKL2s where P.AbsEntry == PickID & P.PickEntry == Pickentry & P.ItemCode == Itemcode & P.SnBEntry == BAbs[0].AbsEntry & P.PickQtty >= Qty select P).ToList();
                //    if (P2.Count > 0)
                //    {
                //        i = 1;
                //    }
                //}
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(i);
        }

        [HttpGet]
        [Route("api/WebApi/CheckPickedSerial")] // Completed Database
        public IHttpActionResult CheckPickedSerial(int PickID, int Pickentry, string Itemcode, string Serial)
        {
            int i = 0;
            try
            {
                i = wmsServiceClient.CheckPickedSerial(PickID, Pickentry, Itemcode, Serial);
                //var SAbs = (from S in newDBContext.OSRNs where S.DistNumber == Serial & S.ItemCode == Itemcode select new { S.AbsEntry }).ToList();
                //if (SAbs.Count > 0)
                //{
                //    var P2 = (from P in newDBContext.PKL2s where P.AbsEntry == PickID & P.PickEntry == Pickentry & P.ItemCode == Itemcode & P.SnBEntry == SAbs[0].AbsEntry select P).ToList();
                //    if (P2.Count > 0)
                //    {
                //        i = 1;
                //    }
                //}
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(i);
        }

        [HttpPost]
        [Route("api/WebApi/Pick")] // Complted Database
        public IHttpActionResult Pick(int absentry, List<GRLineDetails> lineDetails)
        {
            string Int_result = "Success";
            try
            {
                Int_result = wmsServiceClient.Allocate_TOPicklist(absentry, ConvertGRLineDetailsToServiceList(lineDetails));
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                Int_result = errmsg;
                return Ok(errmsg);
            }
            return Ok(Int_result);
        }

        [HttpPost]
        [Route("api/WebApi/Pack")] // Completed Complted
        public IHttpActionResult Pack(int absentry, List<GRBoxDetails> BoxDtls)
        {
            string Int_result = "Success";
            try
            {
                Int_result = wmsServiceClient.Pack(absentry, ConvertGRBoxDetailsToServiceList(BoxDtls));
                //if (BoxDtls.Count > 0)
                //{
                //    int t;
                //    foreach (var BL in BoxDtls)
                //    {
                //        if (BL.ItemType == 'N')
                //        {

                //            var PK = (from p in newDBContext.PKL2s where p.AbsEntry == absentry & p.PickEntry == BL.PickLine & p.ItemCode == BL.Item select new { p.BinAbs, p.SnBEntry }).ToList();
                //            var Sonum = (from s in newDBContext.ORDRs where s.DocEntry == BL.SoDocEntry select s.DocNum).ToList();
                //            var Bin = (from b in newDBContext.OBINs where b.AbsEntry == PK[0].BinAbs select b.BinCode).ToList();

                //            using (SqlConnection con = new SqlConnection(newDBContext.Connection.ConnectionString))
                //            {
                //                using (SqlCommand cmd = new SqlCommand("USP_WMS35_InsertBoxDetails"))
                //                {
                //                    cmd.CommandType = CommandType.StoredProcedure;
                //                    cmd.Connection = con;
                //                    cmd.Parameters.AddWithValue("@ID", BL.ID);
                //                    cmd.Parameters.AddWithValue("@SoNum", BL.SoNum);
                //                    cmd.Parameters.AddWithValue("@SoDocEntry", BL.SoDocEntry);
                //                    cmd.Parameters.AddWithValue("@SoDocLine", BL.SoDocLine);
                //                    cmd.Parameters.AddWithValue("@Picklist", BL.PickList);
                //                    cmd.Parameters.AddWithValue("@PickLine", BL.PickLine);
                //                    cmd.Parameters.AddWithValue("@Item", BL.Item);
                //                    cmd.Parameters.AddWithValue("@Batch", DBNull.Value);
                //                    cmd.Parameters.AddWithValue("@Serial", DBNull.Value);
                //                    cmd.Parameters.AddWithValue("@SnB", BL.SnB);
                //                    cmd.Parameters.AddWithValue("@Qty", BL.Qty);
                //                    cmd.Parameters.AddWithValue("@ExpDate", DBNull.Value);
                //                    cmd.Parameters.AddWithValue("@BinID", BL.BinID);
                //                    cmd.Parameters.AddWithValue("@BinAbs", BL.BinAbs);
                //                    cmd.Parameters.AddWithValue("@Manuf", "");
                //                    cmd.Parameters.AddWithValue("@GTIN", "");
                //                    cmd.Parameters.AddWithValue("@ExpDateStr", "");
                //                    cmd.Parameters.AddWithValue("@PallID", BL.PallID);
                //                    con.Open();
                //                    t = cmd.ExecuteNonQuery();
                //                    con.Close();
                //                }
                //            }
                //        }
                //        else if (BL.ItemType == 'S')
                //        {
                //            var Sonum = (from s in newDBContext.ORDRs where s.DocEntry == BL.SoDocEntry select s.DocNum).ToList();
                //            if (BL.Serial == "")
                //            {
                //                var BS = (from B in newDBContext._WMS35_BOXLs
                //                          join o in newDBContext.OSRNs on B.U_SerialNo equals o.DistNumber
                //                          where B.U_PickID == absentry & B.U_PickEntry == BL.PickLine & B.U_ItemCode == BL.Item
                //                          select o.AbsEntry).ToList();

                //                var PK = (from p in newDBContext.PKL2s where p.AbsEntry == absentry & p.PickEntry == BL.PickLine & p.ItemCode == BL.Item select new { p.SnBEntry, p.BinAbs }).ToList();
                //                var Sr = (from P1 in PK where !BS.Contains((int)P1.SnBEntry) select P1).ToList();

                //                for (int j = 0; j <= BL.Qty - 1; j++)
                //                {
                //                    var Ser = (from o in newDBContext.OSRNs where o.AbsEntry == Convert.ToInt32(Sr[j].SnBEntry) select new { o.DistNumber, o.LotNumber, o.U_Manufacturer, o.U_GTIN, o.ExpDate, o.U_ExpDateStr }).ToList();
                //                    var Bin = (from b in newDBContext.OBINs where b.AbsEntry == Sr[j].BinAbs select b.BinCode).ToList();

                //                    using (SqlConnection con = new SqlConnection(newDBContext.Connection.ConnectionString))
                //                    {
                //                        using (SqlCommand cmd = new SqlCommand("USP_WMS35_InsertBoxDetails"))
                //                        {
                //                            cmd.CommandType = CommandType.StoredProcedure;
                //                            cmd.Connection = con;
                //                            cmd.Parameters.AddWithValue("@ID", BL.ID);
                //                            cmd.Parameters.AddWithValue("@SoNum", Convert.ToInt32(Sonum[0].ToString()));
                //                            cmd.Parameters.AddWithValue("@SoDocEntry", BL.SoDocEntry);
                //                            cmd.Parameters.AddWithValue("@SoDocLine", BL.SoDocLine);
                //                            cmd.Parameters.AddWithValue("@Picklist", BL.PickList);
                //                            cmd.Parameters.AddWithValue("@PickLine", BL.PickLine);
                //                            cmd.Parameters.AddWithValue("@Item", BL.Item);
                //                            cmd.Parameters.AddWithValue("@Batch", DBNull.Value);
                //                            cmd.Parameters.AddWithValue("@Serial", Ser[0].DistNumber);
                //                            cmd.Parameters.AddWithValue("@SnB", Sr[j].SnBEntry);
                //                            cmd.Parameters.AddWithValue("@Qty", 1);
                //                            cmd.Parameters.AddWithValue("@ExpDate", Ser[0].ExpDate);
                //                            cmd.Parameters.AddWithValue("@BinID", Bin[0].ToString());
                //                            cmd.Parameters.AddWithValue("@BinAbs", Sr[j].BinAbs);
                //                            cmd.Parameters.AddWithValue("@Manuf", Ser[0].U_Manufacturer);
                //                            cmd.Parameters.AddWithValue("@GTIN", Ser[0].U_GTIN);
                //                            cmd.Parameters.AddWithValue("@ExpDateStr", Ser[0].U_ExpDateStr);
                //                            cmd.Parameters.AddWithValue("@PallID", BL.PallID);
                //                            con.Open();
                //                            t = cmd.ExecuteNonQuery();
                //                            con.Close();
                //                        }
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                var SrAbs = (from osr in newDBContext.OSRNs where osr.DistNumber == BL.Serial select new { osr.AbsEntry, osr.LotNumber, osr.U_Manufacturer, osr.U_GTIN, osr.ExpDate, osr.U_ExpDateStr }).FirstOrDefault();
                //                var PK = (from p in newDBContext.PKL2s where p.AbsEntry == absentry & p.PickEntry == BL.PickLine & p.ItemCode == BL.Item & p.SnBEntry == SrAbs.AbsEntry select new { p.BinAbs, p.SnBEntry }).ToList();
                //                var Bin = (from b in newDBContext.OBINs where b.AbsEntry == PK[0].BinAbs select b.BinCode).ToList();
                //                using (SqlConnection con = new SqlConnection(newDBContext.Connection.ConnectionString))
                //                {
                //                    using (SqlCommand cmd = new SqlCommand("USP_WMS35_InsertBoxDetails"))
                //                    {
                //                        cmd.CommandType = CommandType.StoredProcedure;
                //                        cmd.Connection = con;
                //                        cmd.Parameters.AddWithValue("@ID", BL.ID);
                //                        cmd.Parameters.AddWithValue("@SoNum", Convert.ToInt32(Sonum[0].ToString()));
                //                        cmd.Parameters.AddWithValue("@SoDocEntry", BL.SoDocEntry);
                //                        cmd.Parameters.AddWithValue("@SoDocLine", BL.SoDocLine);
                //                        cmd.Parameters.AddWithValue("@Picklist", BL.PickList);
                //                        cmd.Parameters.AddWithValue("@PickLine", BL.PickLine);
                //                        cmd.Parameters.AddWithValue("@Item", BL.Item);
                //                        cmd.Parameters.AddWithValue("@Batch", DBNull.Value);
                //                        cmd.Parameters.AddWithValue("@Serial", BL.Serial);
                //                        cmd.Parameters.AddWithValue("@SnB", Convert.ToInt32(PK[0].SnBEntry));
                //                        cmd.Parameters.AddWithValue("@Qty", BL.Qty);
                //                        cmd.Parameters.AddWithValue("@ExpDate", SrAbs.ExpDate);
                //                        cmd.Parameters.AddWithValue("@BinID", Bin[0].ToString());
                //                        cmd.Parameters.AddWithValue("@BinAbs", Convert.ToInt32(PK[0].BinAbs));
                //                        cmd.Parameters.AddWithValue("@Manuf", SrAbs.U_Manufacturer);
                //                        cmd.Parameters.AddWithValue("@GTIN", SrAbs.U_GTIN);
                //                        cmd.Parameters.AddWithValue("@ExpDateStr", SrAbs.U_ExpDateStr);
                //                        cmd.Parameters.AddWithValue("@PallID", BL.PallID);
                //                        con.Open();
                //                        t = cmd.ExecuteNonQuery();
                //                        con.Close();
                //                    }
                //                }
                //            }
                //        }
                //        else if (BL.ItemType == 'B')
                //        {
                //            var Sonum = (from s in newDBContext.ORDRs where s.DocEntry == BL.SoDocEntry select s.DocNum).ToList();
                //            if (BL.Batch == "")
                //            {
                //                var BS = (from B in newDBContext._WMS35_BOXLs
                //                          join o in newDBContext.OBTNs on B.U_BatchNo equals o.DistNumber
                //                          where B.U_PickID == absentry & B.U_PickEntry == BL.PickLine & B.U_ItemCode == BL.Item
                //                          select o.AbsEntry).ToList();
                //                var PK = (from p in newDBContext.PKL2s where p.AbsEntry == absentry & p.PickEntry == BL.PickLine & p.ItemCode == BL.Item select new { p.SnBEntry, p.PickQtty, p.BinAbs }).ToList();
                //                var Bt = (from P1 in PK where !BS.Contains((int)P1.SnBEntry) select P1).ToList();
                //                var Bin = (from b in newDBContext.OBINs where b.AbsEntry == PK[0].BinAbs select b.BinCode).ToList();
                //                for (int i = 0; i <= Bt.Count - 1; i++)
                //                {
                //                    var Bat = (from o in newDBContext.OBTNs where o.AbsEntry == Bt[i].SnBEntry select new { o.DistNumber, o.ExpDate }).ToList();
                //                    using (SqlConnection con = new SqlConnection(newDBContext.Connection.ConnectionString))
                //                    {
                //                        using (SqlCommand cmd = new SqlCommand("USP_WMS35_InsertBoxDetails"))
                //                        {
                //                            cmd.CommandType = CommandType.StoredProcedure;
                //                            cmd.Connection = con;
                //                            cmd.Parameters.AddWithValue("@ID", BL.ID);
                //                            cmd.Parameters.AddWithValue("@SoNum", Convert.ToInt32(Sonum[0].ToString()));
                //                            cmd.Parameters.AddWithValue("@SoDocEntry", BL.SoDocEntry);
                //                            cmd.Parameters.AddWithValue("@SoDocLine", BL.SoDocLine);
                //                            cmd.Parameters.AddWithValue("@Picklist", BL.SoNum);
                //                            cmd.Parameters.AddWithValue("@PickLine", BL.PickLine);
                //                            cmd.Parameters.AddWithValue("@Item", BL.Item);
                //                            cmd.Parameters.AddWithValue("@Batch", Bat[0].DistNumber);
                //                            cmd.Parameters.AddWithValue("@Serial", DBNull.Value);
                //                            cmd.Parameters.AddWithValue("@SnB", Bt[i].SnBEntry);
                //                            cmd.Parameters.AddWithValue("@Qty", Bt[i].PickQtty);
                //                            cmd.Parameters.AddWithValue("@ExpDate", Bat[0].ExpDate);
                //                            cmd.Parameters.AddWithValue("@BinID", Bin[0].ToString());
                //                            cmd.Parameters.AddWithValue("@BinAbs", Bt[i].BinAbs);
                //                            cmd.Parameters.AddWithValue("@Manuf", "");
                //                            cmd.Parameters.AddWithValue("@GTIN", "");
                //                            cmd.Parameters.AddWithValue("@ExpDateStr", "");
                //                            cmd.Parameters.AddWithValue("@PallID", BL.PallID);
                //                            con.Open();
                //                            t = cmd.ExecuteNonQuery();
                //                            con.Close();
                //                        }
                //                    }
                //                }
                //            }
                //            else
                //            {
                //                var BtAbs = (from obt in newDBContext.OBTNs where obt.DistNumber == BL.Batch select new { obt.AbsEntry, obt.ExpDate }).FirstOrDefault();
                //                var PK = (from p in newDBContext.PKL2s where p.AbsEntry == absentry & p.PickEntry == BL.PickLine & p.ItemCode == BL.Item & p.SnBEntry == BtAbs.AbsEntry select new { p.BinAbs, p.SnBEntry }).ToList();
                //                var Bin = (from b in newDBContext.OBINs where b.AbsEntry == PK[0].BinAbs select b.BinCode).ToList();
                //                using (SqlConnection con = new SqlConnection(newDBContext.Connection.ConnectionString))
                //                {
                //                    using (SqlCommand cmd = new SqlCommand("USP_WMS35_InsertBoxDetails"))
                //                    {
                //                        cmd.CommandType = CommandType.StoredProcedure;
                //                        cmd.Connection = con;
                //                        cmd.Parameters.AddWithValue("@ID", BL.ID);
                //                        cmd.Parameters.AddWithValue("@SoNum", Convert.ToInt32(Sonum[0].ToString()));
                //                        cmd.Parameters.AddWithValue("@SoDocEntry", BL.SoDocEntry);
                //                        cmd.Parameters.AddWithValue("@SoDocLine", BL.SoDocLine);
                //                        cmd.Parameters.AddWithValue("@Picklist", BL.PickList);
                //                        cmd.Parameters.AddWithValue("@PickLine", BL.PickLine);
                //                        cmd.Parameters.AddWithValue("@Item", BL.Item);
                //                        cmd.Parameters.AddWithValue("@Batch", BL.Batch);
                //                        cmd.Parameters.AddWithValue("@Serial", DBNull.Value);
                //                        cmd.Parameters.AddWithValue("@SnB", Convert.ToInt32(PK[0].SnBEntry));
                //                        cmd.Parameters.AddWithValue("@Qty", BL.Qty);
                //                        cmd.Parameters.AddWithValue("@ExpDate", BtAbs.ExpDate);
                //                        cmd.Parameters.AddWithValue("@BinID", Bin[0].ToString());
                //                        cmd.Parameters.AddWithValue("@BinAbs", Convert.ToInt32(PK[0].BinAbs));
                //                        cmd.Parameters.AddWithValue("@Manuf", "");
                //                        cmd.Parameters.AddWithValue("@GTIN", "");
                //                        cmd.Parameters.AddWithValue("@ExpDateStr", "");
                //                        cmd.Parameters.AddWithValue("@PallID", BL.PallID);
                //                        con.Open();
                //                        t = cmd.ExecuteNonQuery();
                //                        con.Close();
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(Int_result);
        }

        [HttpPost]
        [Route("api/WebApi/CreateGRPO")] // Pending - write new logic
        public IHttpActionResult CreateGRPO([FromBody] GRRequest baseRequest)
        {
            string Int_result = "Success";
            try
            {
                Int_result = wmsServiceClient.Create_GRPO(ConvertGRLineDetailsToServiceList(baseRequest.LineDetails), ConvertGRBoxDetailsToServiceList(baseRequest.BoxDetails), baseRequest.CanAutomateSalesOrder);
                //if(!Int_result.Equals("Success"))
                //    return Content(System.Net.HttpStatusCode.Conflict, Int_result);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Ok(errmsg);
            }
            return Ok(Int_result);
        }

        [HttpPost]
        [Route("api/WebApi/CreateGoodsReceipt")] // Pending - write new logic
        public IHttpActionResult CreateGoodsReceipt([FromBody] GRRequest baseRequest)
        {
            string Int_result = "Success";
            try
            {
                Int_result = wmsServiceClient.Create_Goodsreceipt(ConvertGRLineDetailsToServiceList(baseRequest.LineDetails), ConvertGRBoxDetailsToServiceList(baseRequest.BoxDetails));
                //if (!Int_result.Equals("Success"))
                //    return Content(System.Net.HttpStatusCode.Conflict, Int_result);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                Int_result = errmsg;
                return Ok(errmsg);
            }
            return Ok(Int_result);
        }

        [HttpPost]
        [Route("api/WebApi/CreateGoodsIssue")] // Pending - write new logic
        public IHttpActionResult CreateGoodsIssue([FromBody] List<GRLineDetails> lineDetails)
        {
            string Int_result = "Success";
            try
            {
                Int_result = wmsServiceClient.Create_Goods_Issue(ConvertGRLineDetailsToServiceList(lineDetails));
                //if (!Int_result.Equals("Success"))
                //    return Content(System.Net.HttpStatusCode.Conflict, Int_result);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                Int_result = errmsg;
                return Ok(errmsg);
            }
            return Ok(Int_result);
        }

        [HttpPost]
        [Route("api/WebApi/CreateStockTransfer")] // Pending - write new logic
        public IHttpActionResult CreateStockTransfer([FromBody] List<GRLineDetails> lineDetails)
        {
            string Int_result = "Success";
            try
            {
                Int_result = wmsServiceClient.Create_Stocktransfer(ConvertGRLineDetailsToServiceList(lineDetails));
                //if(!Int_result.Equals("Success"))
                //    return Content(System.Net.HttpStatusCode.Conflict, Int_result);

            }
            catch (Exception e)
            {
                errmsg = e.Message;
                Int_result = errmsg;
                return Ok(errmsg);
            }
            return Ok(Int_result);
        }


        [HttpPost]
        [Route("api/WebApi/CreateDelivery")]
        public IHttpActionResult CreateDelivery(int pickId, string userName)
        {
            string Int_result = "Success";
            try
            {
                Int_result = wmsServiceClient.createDelivery(pickId, userName);
                //if(!Int_result.Equals("Success"))
                //    return Content(System.Net.HttpStatusCode.Conflict, Int_result);

            }
            catch (Exception e)
            {
                errmsg = e.Message;
                Int_result = errmsg;
                return Ok(errmsg);
            }
            return Ok(Int_result);
        }


        [HttpGet]
        [Route("api/WebApi/GetBinForItem")] 
        public IHttpActionResult GetBinForItem(string itemCode)
        {
            List<string> result = new List<string>();
            try
            {
                var serviceResult = wmsServiceClient.GetBinForItem(itemCode).ToList();
                if(serviceResult.Any())
                {
                    result.AddRange(serviceResult);
                }
                else
                    return Content(System.Net.HttpStatusCode.NotFound, "Not found");
                //var itemType = (from O in newDBContext.OITMs
                //                where O.ItemCode == itemCode
                //                select new { O.ManBtchNum, O.ManSerNum }).ToList().First();

                //if(itemType.ManBtchNum.Value == 'N' && itemType.ManSerNum.Value == 'N') // General
                //{
                //    var queryResult = (from T0 in newDBContext.OIBQs
                //                       join T1 in newDBContext.OITMs on T0.ItemCode equals T1.ItemCode                                       
                //                       join T3 in newDBContext.OBINs on T0.BinAbs equals T3.AbsEntry
                //                       where T0.ItemCode == itemCode
                //                       select new { BinCode = T3.BinCode }).ToList();
                //    if (queryResult != null && queryResult.Any())
                //    {
                //        queryResult.ToList().ForEach(x =>
                //        {
                //            if (!result.Contains(x.BinCode))
                //                result.Add(x.BinCode);
                //        });
                //    }
                //}
                //else if (itemType.ManBtchNum.Value == 'Y' && itemType.ManSerNum.Value == 'N') // Batch
                //{
                //    var queryResult = (from T0 in newDBContext.OBBQs
                //                       join T1 in newDBContext.OITMs on T0.ItemCode equals T1.ItemCode
                //                       join T2 in newDBContext.OBTNs on T0.SnBMDAbs equals T2.AbsEntry
                //                       join T3 in newDBContext.OBINs on T0.BinAbs equals T3.AbsEntry
                //                       where T0.ItemCode == itemCode
                //                       select new { BinCode = T3.BinCode }).ToList();
                //    if (queryResult != null && queryResult.Any())
                //    {
                //        queryResult.ToList().ForEach(x =>
                //        {
                //            if (!result.Contains(x.BinCode))
                //                result.Add(x.BinCode);
                //        });
                //    }
                //}
                //else if (itemType.ManBtchNum.Value == 'N' && itemType.ManSerNum.Value == 'Y') // Serial
                //{
                //    var queryResult = (from T0 in newDBContext.OSBQs
                //                       join T1 in newDBContext.OITMs on T0.ItemCode equals T1.ItemCode
                //                       join T2 in newDBContext.OSRNs on T0.SnBMDAbs equals T2.AbsEntry
                //                       join T3 in newDBContext.OBINs on T0.BinAbs equals T3.AbsEntry
                //                       where T0.ItemCode == itemCode
                //                       select new { BinCode = T3.BinCode }).ToList();
                //    if (queryResult != null && queryResult.Any())
                //    {
                //        queryResult.ToList().ForEach(x =>
                //        {
                //            if (!result.Contains(x.BinCode))
                //                result.Add(x.BinCode);
                //        });
                //    }
                //}
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/GetDeliveryItemDetails")]
        public IHttpActionResult GetDeliveryItemDetails(int pickID)
        {
            List<DeliveryListDetails> result = new List<DeliveryListDetails>();
            try
            {
                var serviceResponse = wmsServiceClient.GetCustomerByPickId(pickID);
                var custDetails = serviceResponse.Split(',');
                var res = wmsServiceClient.GetBoxByPickId(pickID);
                if (res != null && res.Any())
                {
                    foreach (var item in res)
                    {
                        DeliveryListDetails detail = new DeliveryListDetails();
                        detail.BoxName = item.GBU_BoxUID;
                        detail.BoxStatus = item.GBU_BoxST;
                        detail.CustCode = custDetails[0];
                        detail.CustName = custDetails[1];
                        detail.ItemDescription = "";
                        detail.ItemName = "";
                        detail.Quantity = "";
                        result.Add(detail);
                    }
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/ValidateAutomaticAllocation")]
        public IHttpActionResult ValidateAutomaticAllocation(int poNumber)
        {
            List<int> result = new List<int>();
            try
            {
                result = wmsServiceClient.Validate_AutomaticAllocation(poNumber)?.ToList();
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }


        [HttpGet]
        [Route("api/WebApi/GetAvailableQuantity")]
        public IHttpActionResult GetAvailableQuantity(int pickId, int docEntry, string itemCode, int linenumber)
        {
            List<AvailableQuantityToPick> result = new List<AvailableQuantityToPick>();
            try
            {
                var serviceResponse = wmsServiceClient.GetAvailableQuantityToPack(pickId, docEntry, itemCode, linenumber);
                if(serviceResponse != null && serviceResponse.Any())
                {
                    serviceResponse.ToList().ForEach(x =>
                    {
                        var res = new AvailableQuantityToPick();
                        res.AllocatedQuantity = x.AllocatedQty;
                        res.AvailableQuantity = x.AvailableQty;
                        res.Batch = x.Batch;
                        res.PickedQuantity = x.PickedQty;
                        result.Add(res);
                    });
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/PrintDeliveryNotePackingSlip")]
        public IHttpActionResult PrintDeliveryNotePackingSlip(string deliveryDocumentNumber)
        {
            string result = "";
            try
            {
                result = wmsServiceClient.PrintDnotePslip(deliveryDocumentNumber);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/GetCaseLableItemCode")]
        public IHttpActionResult GetCaseLableItemCode(string itemDescription, int noOfCopies, int pOOrderNumber, string userName)
        {
            string result = "";
            try
            {
                result = wmsServiceClient.Get_caselabel_itemcode(itemDescription, noOfCopies, pOOrderNumber, userName);
                if (!result.ToString().Equals("Printed"))
                {
                    result = "Label Printing failed.";
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        [HttpPost]
        [Route("api/WebApi/GetCaseLabelBox")]
        public IHttpActionResult GetCaseLabelBox([FromBody] PrintBoxDetail printBoxDetail)
        {
            string result = "";
            try
            {
                result = wmsServiceClient.Get_caselabel_box(printBoxDetail.BoxIds.ToArray(), printBoxDetail.NoOfCopies, printBoxDetail.PoOrderNumber, printBoxDetail.UserName);
                if (!result.ToString().Equals("Printed"))
                {
                    result = "Label Printing failed.";
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/PrintPicklist")]
        public IHttpActionResult PrintPicklist(int absEntry)
        {
            string result = "";
            try
            {
                result = wmsServiceClient.PrintPicklist(absEntry);
                if (!result.ToString().Equals("Success"))
                {
                    result = "Printing failed.";
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }


        [HttpGet]
        [Route("api/WebApi/CheckItemStatus")]
        public IHttpActionResult CheckItemStatus(string itemCode)
        {
            string result = "";
            try
            {
                result = wmsServiceClient.CheckItem(itemCode); // Y - Inactive, N - Active
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/CheckBusinessPartner")]
        public IHttpActionResult CheckBusinessPartner(string cardCode, string cardType)
        {
            string result = "";
            try
            {
                result = wmsServiceClient.CheckBusinessPartner(cardCode, cardType); // Y - Inactive, N - Active
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/GetPOBin")]
        public IHttpActionResult GetPOBin(int pONumber)
        {
            List<string> result = new List<string>();
            try
            {
                var serviceResponse = wmsServiceClient.GetPOBin(pONumber); // Empty or Bin Name
                if(serviceResponse != null && serviceResponse.Any())
                {
                    result.AddRange(serviceResponse);
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/GetAllocation")]
        public IHttpActionResult GetAllocation(int pickId, int docEntry, string itemCode, int lineNumber, string from)
        {
            List<AllocationDetails> result = new List<AllocationDetails>();
            try
            {
                var serviceResponse = wmsServiceClient.Get_Allocation(pickId, docEntry, itemCode, lineNumber, from); // Empty or Bin Name
                if (serviceResponse != null && serviceResponse.Any())
                {
                    foreach(var response in serviceResponse)
                    {
                        var allocationDetail = new AllocationDetails();
                        allocationDetail.AbsEntry = response.AbsEntry;
                        allocationDetail.AllocatedQuantity = response.AllocatedQty.GetValueOrDefault();
                        allocationDetail.Batch = response.Batch;
                        allocationDetail.ItemCode = response.Itemcode;
                        allocationDetail.LineNumber = response.Linenum.GetValueOrDefault();
                        allocationDetail.SystemNumber = response.SysNumber;
                        result.Add(allocationDetail);
                    }                                   
                }
                else
                {
                    result = null;
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/GetPickListLineBinDetails")]
        public IHttpActionResult GetPickListLineBinDetails(int pickId, int lineNumber)
        {
            List<string> result = new List<string>();
            try
            {
                var serviceResponse = wmsServiceClient.Get_Picklist_line_bin(pickId, lineNumber);
                if (serviceResponse != null && serviceResponse.Any())
                {
                    result.AddRange(serviceResponse);
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        //

        [HttpGet]
        [Route("api/WebApi/GetItemWHSStock")]
        public IHttpActionResult GetItemWHSStock(string itemCode)
        {
            string result = string.Empty;
            try
            {
                var serviceResponse = wmsServiceClient.GetItemWHSStock(itemCode);
                if (!string.IsNullOrEmpty(serviceResponse))
                {
                    result = serviceResponse;
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/GetItemBinWhsStock")]
        public IHttpActionResult GetItemBinWhsStock(string itemCode, int binAbs, string whs)
        {
            List<string> result = new List<string>();
            try
            {
                var serviceResponse = wmsServiceClient.GetItemBinWhsStock(itemCode, binAbs, whs);
                if (serviceResponse != null && serviceResponse.Any())
                {
                    result.AddRange(serviceResponse);
                }
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        [HttpGet]
        [Route("api/WebApi/Print")]
        public IHttpActionResult Print(string deliveryDocumentNumber)
        {
            string result = "";
            try
            {
                result = wmsServiceClient.Print(deliveryDocumentNumber);
            }
            catch (Exception e)
            {
                errmsg = e.Message;
                return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
            }
            return Ok(result);
        }

        //[HttpGet]
        //[Route("api/WebApi/CheckBin")]
        //public IHttpActionResult CheckBin(string bin, string whs)
        //{
        //    int result = default(int);
        //    try
        //    {
        //        result = wmsServiceClient.CheckBin(bin, whs);
        //    }
        //    catch (Exception e)
        //    {
        //        errmsg = e.Message;
        //        return Content(System.Net.HttpStatusCode.InternalServerError, errmsg);
        //    }
        //    return Ok(result);
        //}


        private WmsService.Get_LineDetails[] ConvertGRLineDetailsToServiceList(List<GRLineDetails> lineDetails)
        {
            var result = new List<WmsService.Get_LineDetails>();
            
            lineDetails.ForEach(x =>
            {
                var data = new WmsService.Get_LineDetails();
                data.Binabsentry = x.Binabsentry;
                data.Docentry = x.Docentry;
                data.Inserialnumber = x.Inserialnumber;
                data.Itemcode = x.Itemcode;
                data.Itemdiscription = x.Itemdiscription;
                data.Itemmngbysernum = x.Itemmngbysernum;
                data.Itemprice = x.Itemprice;
                data.Linenumber = x.Linenumber;
                data.Linestatus = x.Linestatus;
                data.Mnfserialnumber = x.Mnfserialnumber;
                data.Multitem = x.Multitem;
                data.Onhanditemqty = x.Onhanditemqty;
                data.Pickid = x.Pickid;
                data.Quantity = x.Quantity;
                data.Releaseqty = x.Releaseqty;
                data.serBinabsentry = x.serBinabsentry;
                data.SoDocentry = x.SoDocentry;
                data.SoDocNum = x.SoDocNum;
                data.Username = x.Username;
                data.Visorder = x.Visorder;
                data.Whscode = x.Whscode;
                result.Add(data);
            });
            return result.ToArray();
        }

        private WmsService.Get_Boxl[] ConvertGRBoxDetailsToServiceList(List<GRBoxDetails> boxDetails)
        {
            var result = new List<WmsService.Get_Boxl>();            
            boxDetails.ForEach(x =>
            {
                var data = new WmsService.Get_Boxl();
                data.GLcode = x.GLcode;
                data.GLname = x.GLname;
                data.GLU_AddedToDn = x.GLU_AddedToDn;
                data.GLU_BatchNo = x.GLU_BatchNo;
                data.GLU_BinAbsEntry = x.GLU_BinAbsEntry;
                data.GLU_BinID = x.GLU_BinID;
                data.GLU_ExpDateStr = x.GLU_ExpDateStr;
                data.GLU_ExpiryDate = x.GLU_ExpiryDate;
                data.GLU_GTIN = x.GLU_GTIN;
                data.GLU_ID = x.GLU_ID;
                data.GLU_IsDeleted = x.GLU_IsDeleted;
                data.GLU_ItemCode = x.GLU_ItemCode;
                data.GLU_Manuf = x.GLU_Manuf;
                data.GLU_PoDocEntry = x.GLU_PoDocEntry;
                data.GLU_PoNum = x.GLU_PoNum;
                data.GLU_PoNVerify = x.GLU_PoNVerify;
                data.GLU_PoNVerifyStatus = x.GLU_PoNVerifyStatus;
                data.GLU_PoObjType = x.GLU_PoObjType;
                data.GLU_Qty = x.GLU_Qty;
                data.GLU_SerialNo = x.GLU_SerialNo;
                data.GLU_SoDocEntry = x.GLU_SoDocEntry;
                data.GLU_SoNum = x.GLU_SoNum;
                data.GLU_SoNVerify = x.GLU_SoNVerify;
                data.GLU_SoNVerifyStatus = x.GLU_SoNVerifyStatus;
                data.GLU_SoObjType = x.GLU_SoObjType;
                data.LineNumber = x.LineNumber;
                
                result.Add(data);
            });
            return result.ToArray();
        }
    }
}
