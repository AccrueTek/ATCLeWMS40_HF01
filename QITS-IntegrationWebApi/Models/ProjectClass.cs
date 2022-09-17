using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QITS_IntegrationWebApi.Models
{
    public class UserTable
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class DocHeader
    {
        public int DocEntry { get; set; }
        public int DocNum { get; set; }
        public string DocType { get; set; }
        public string DocStatus { get; set; }
        public string ObjType { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public string CardCode { get; set; }
        public string CardName { get; set; }
        public decimal DocTotal { get; set; }
        public string Comments { get; set; }
        public int UserId { get; set; }
        public string Filler { get; set; }
        public string ToWHS { get; set; }
        public string IntDB { get; set; }
        public List<LineDetails> GRPOLDtls { get; set; }
        public List<ItemDetails> GRPOIDtls { get; set; }
    }

    public class GRRequest
    {
        public List<GRLineDetails> LineDetails { get; set; }

        public List<GRBoxDetails> BoxDetails { get; set; }

        public bool CanAutomateSalesOrder { get; set; }
    }

    public class GRLineDetails
    {
        private int _docentry = 0;
        private string _itemcode = string.Empty;
        private decimal _quantity = 0;
        private string _whscode = string.Empty;
        private int _linenumber = 0;
        private string _linestatus = string.Empty;
        private decimal _onhanditemqty = 0;
        private string _itemmngbysernum = string.Empty;
        private string _inserialnumber = string.Empty;
        private string _mnfserialnumber = string.Empty;
        private string _binabsentry = string.Empty;
        private string _serbinabsentry = string.Empty;

        private string _itemdiscription = string.Empty;
        private string _itemprice = string.Empty;
        private string _username = string.Empty;
        private string _pickid = string.Empty;
        private decimal _releaseqty = 0;
        private string _visorder = string.Empty;
        private string _multitem = string.Empty;
        private int _SoDocNum = 0;
        private int _Sodocentry = 0;


        public int Docentry
        {
            get
            {
                return _docentry;
            }
            set
            {
                _docentry = value;
            }
        }

        public string Itemcode
        {
            get
            {
                return _itemcode;
            }
            set
            {
                _itemcode = value;
            }
        }
        public decimal Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                _quantity = value;
            }
        }
        public string Whscode
        {
            get
            {
                return _whscode;
            }
            set
            {
                _whscode = value;
            }
        }
        public int Linenumber
        {
            get
            {
                return _linenumber;
            }
            set
            {
                _linenumber = value;
            }
        }
        public string Linestatus
        {
            get
            {
                return _linestatus;
            }
            set
            {
                _linestatus = value;
            }
        }
        public decimal Onhanditemqty
        {
            get
            {
                return _onhanditemqty;
            }
            set
            {
                _onhanditemqty = value;
            }
        }


        public string Itemmngbysernum
        {
            get
            {
                return _itemmngbysernum;
            }
            set
            {
                _itemmngbysernum = value;
            }
        }

        public string Inserialnumber
        {
            get
            {
                return _inserialnumber;
            }
            set
            {
                _inserialnumber = value;
            }
        }
        public string Mnfserialnumber
        {
            get
            {
                return _mnfserialnumber;
            }
            set
            {
                _mnfserialnumber = value;
            }
        }


        public string Binabsentry
        {
            get
            {
                return _binabsentry;
            }
            set
            {
                _binabsentry = value;
            }
        }

        public string serBinabsentry
        {
            get
            {
                return _serbinabsentry;
            }
            set
            {
                _serbinabsentry = value;
            }
        }

        public string Itemdiscription
        {
            get
            {
                return _itemdiscription;
            }
            set
            {
                _itemdiscription = value;
            }
        }


        public string Itemprice
        {
            get
            {
                return _itemprice;
            }
            set
            {
                _itemprice = value;
            }
        }


        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
            }
        }


        public string Pickid
        {
            get
            {
                return _pickid;
            }
            set
            {
                _pickid = value;
            }
        }
        public decimal Releaseqty
        {
            get
            {
                return _releaseqty;
            }
            set
            {
                _releaseqty = value;
            }
        }

        public string Visorder
        {
            get
            {
                return _visorder;
            }
            set
            {
                _visorder = value;
            }
        }

        public string Multitem
        {
            get
            {
                return _multitem;
            }
            set
            {
                _multitem = value;
            }
        }

        public int SoDocentry
        {
            get
            {
                return _Sodocentry;
            }
            set
            {
                _Sodocentry = value;
            }
        }

        public int SoDocNum
        {
            get
            {
                return _SoDocNum;
            }
            set
            {
                _SoDocNum = value;
            }
        }
    }

    public class GRBoxDetails
    {
        private string _Code = string.Empty;
        private string _Name = string.Empty;
        private int _U_ID = 0;
        private int _U_SoNum = 0;
        private int _U_SoDocEntry = 0;
        private int _U_SoObjType = 0;
        private int _U_PoNum = 0;
        private int _U_PoDocEntry = 0;
        private int _U_PoObjType = 0;
        private string _U_ItemCode = string.Empty;
        private string _U_BatchNo = string.Empty;
        private string _U_SerialNo = string.Empty;
        private int _U_Qty = 0;
        private int _U_ExpiryDate = 0;
        private string _U_BinID = string.Empty;
        private int _U_BinAbsEntry = 0;
        private string _U_IsDeleted = string.Empty;
        private string _U_AddedToDn = string.Empty;
        private string _U_SoNVerify = string.Empty;
        private string _U_SoNVerifyStatus = string.Empty;
        private string _U_PoNVerify = string.Empty;
        private string _U_PoNVerifyStatus = string.Empty;
        private string _U_Manuf = string.Empty;
        private string _U_GTIN = string.Empty;
        private int _U_ExpDateStr = 0;
        private string _LineNumber = string.Empty;

        public string GLcode
        {
            get
            {
                return _Code;
            }
            set
            {
                _Code = value;
            }
        }

        public string GLname
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        public int GLU_ID
        {
            get
            {
                return _U_ID;
            }
            set
            {
                _U_ID = value;
            }
        }

        public int GLU_SoNum
        {
            get
            {
                return _U_SoNum;
            }
            set
            {
                _U_SoNum = value;
            }
        }

        public int GLU_SoDocEntry
        {
            get
            {
                return _U_SoDocEntry;
            }
            set
            {
                _U_SoDocEntry = value;
            }
        }


        public int GLU_SoObjType
        {
            get
            {
                return _U_SoObjType;
            }
            set
            {
                _U_SoObjType = value;
            }
        }

        public int GLU_PoNum
        {
            get
            {
                return _U_PoNum;
            }
            set
            {
                _U_PoNum = value;
            }
        }

        public int GLU_PoDocEntry
        {
            get
            {
                return _U_PoDocEntry;
            }
            set
            {
                _U_PoDocEntry = value;
            }
        }

        public int GLU_PoObjType
        {
            get
            {
                return _U_PoObjType;
            }
            set
            {
                _U_PoObjType = value;
            }
        }

        public string GLU_ItemCode
        {
            get
            {
                return _U_ItemCode;
            }
            set
            {
                _U_ItemCode = value;
            }
        }

        public string GLU_BatchNo
        {
            get
            {
                return _U_BatchNo;
            }
            set
            {
                _U_BatchNo = value;
            }
        }

        public string GLU_SerialNo
        {
            get
            {
                return _U_SerialNo;
            }
            set
            {
                _U_SerialNo = value;
            }
        }

        public int GLU_Qty
        {
            get
            {
                return _U_Qty;
            }
            set
            {
                _U_Qty = value;
            }
        }

        public int GLU_ExpiryDate
        {
            get
            {
                return _U_ExpiryDate;
            }
            set
            {
                _U_ExpiryDate = value;
            }
        }

        public string GLU_BinID
        {
            get
            {
                return _U_BinID;
            }
            set
            {
                _U_BinID = value;
            }
        }

        public int GLU_BinAbsEntry
        {
            get
            {
                return _U_BinAbsEntry;
            }
            set
            {
                _U_BinAbsEntry = value;
            }
        }

        public string GLU_IsDeleted
        {
            get
            {
                return _U_IsDeleted;
            }
            set
            {
                _U_IsDeleted = value;
            }
        }

        public string GLU_AddedToDn
        {
            get
            {
                return _U_AddedToDn;
            }
            set
            {
                _U_AddedToDn = value;
            }
        }

        public string GLU_SoNVerify
        {
            get
            {
                return _U_SoNVerify;
            }
            set
            {
                _U_SoNVerify = value;
            }
        }

        public string GLU_SoNVerifyStatus
        {
            get
            {
                return _U_SoNVerifyStatus;
            }
            set
            {
                _U_SoNVerifyStatus = value;
            }
        }

        public string GLU_PoNVerify
        {
            get
            {
                return _U_PoNVerify;
            }
            set
            {
                _U_PoNVerify = value;
            }
        }

        public string GLU_PoNVerifyStatus
        {
            get
            {
                return _U_PoNVerifyStatus;
            }
            set
            {
                _U_PoNVerifyStatus = value;
            }
        }

        public string GLU_Manuf
        {
            get
            {
                return _U_Manuf;
            }
            set
            {
                _U_Manuf = value;
            }
        }

        public string GLU_GTIN
        {
            get
            {
                return _U_GTIN;
            }
            set
            {
                _U_GTIN = value;
            }
        }

        public int GLU_ExpDateStr
        {
            get
            {
                return _U_ExpDateStr;
            }
            set
            {
                _U_ExpDateStr = value;
            }
        }

        public string LineNumber
        {
            get
            {
                return _LineNumber;
            }
            set
            {
                _LineNumber = value;
            }
        }
    }


    public class LineDetails
    {
        public int DocEntry { get; set; } //Docentry
        public int LineNum { get; set; } //Linenumber
        public string BaseRef { get; set; }
        public int BaseType { get; set; }
        public int BaseEntry { get; set; }
        public int BaseLine { get; set; }
        public Char LineStatus { get; set; } //Linestatus
        public string ItemCode { get; set; } //Itemcode
        public string Dscription { get; set; }
        public string ItemType { get; set; }
        public string WHS { get; set; } //Whscode
        public string FrmWHS { get; set; }
        public string FrmBin { get; set; }
        public string ToBin { get; set; }
        public int BinAbs { get; set; }
        public decimal Quantity { get; set; }
        public decimal OpenQty { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }

    }
    public class ItemDetails
    {
        public int DocEntry { get; set; }
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public decimal Qty { get; set; }
        public string Batch { get; set; }
        public decimal BatchQty { get; set; }
        public DateTime BatchExpDate { get; set; }
        public string Serial { get; set; }
        public decimal SerialQty { get; set; }
        public DateTime SerialExpDate { get; set; }
        public string WHS { get; set; }
        public int BinAbs { get; set; }
        public decimal BinQty { get; set; }
        public string Gtin { get; set; }
        public string Manuf { get; set; }
        public int ExpDateStr { get; set; }

    }
    public class OpenPOList
    {
        public int PONum { get; set; }
        public int PODocEntry { get; set; }
        public string SupplierCode { get; set; }
        public string SupplierName { get; set; }
        public string SupplierRef { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
    }
    public class PODetails
    {
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public char ItemType { get; set; }
        public int Linenum { get; set; }
        public int OpenQty { get; set; }
        public decimal Price { get; set; }
        public int SoDocEntry { get; set; }
        public int SoDocNum { get; set; }
        public string WHS { get; set; }

    }
    public class PickListDetails
    {
        public int SoDocEntry { get; set; } //SoDocentry
        public int SoNumber { get; set; } //SoDocNum
        public string ItemCode { get; set; } // Itemcode
        public string Description { get; set; } //Itemdiscription
        public int RelQty { get; set; } // Releaseqty
        public int PickQty { get; set; }
        public int Pickentry { get; set; }
        public int OrderLine { get; set; } //Linenumber
        public string WHSCode { get; set; } //Whscode
        public char ItemType { get; set; }

        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string PONumber { get; set; }
        public int SOOpenQuantity { get; set; }

        public string Bin { get; set; }
    }
    public class Box
    {
        public int BoxID { get; set; }
        public string BoxUID { get; set; }
        public char BoxStatus { get; set; }

    }
    public class Pallet
    {
        public int PalletID { get; set; }
        public string PalletUID { get; set; }
        public char PalletStatus { get; set; }

    }
    public class BoxDetails
    {
        public int SoNum { get; set; }
        public int PickId { get; set; }
        public string ItemCode { get; set; }
        public string Description { get; set; }
        public string Batch { get; set; }
        public string Serial { get; set; }
        public string Bin { get; set; }
        public int Qty { get; set; }

    }
    public class BoxDtl
    {
        public int ID { get; set; } // GLU_ID
        public int SoNum { get; set; } //GLU_SoNum
        public int SoDocEntry { get; set; } //GLU_SoDocEntry
        public int SoDocLine { get; set; } //LineNumber
        public int PickList { get; set; }
        public int PickLine { get; set; }
        public string Item { get; set; } //GLU_ItemCode
        public string Batch { get; set; } //GLU_BatchNo
        public string Serial { get; set; } //GLU_SerialNo
        public int SnB { get; set; }
        public int Qty { get; set; } //GLU_Qty
        public DateTime ExpDate { get; set; } //GLU_ExpiryDate
        public string BinID { get; set; } //GLU_BinID
        public int BinAbs { get; set; } //GLU_BinAbsEntry
        public string Manuf { get; set; } //GLU_Manuf
        public string GTIN { get; set; } //GLU_GTIN
        public int ExpDateStr { get; set; } //GLU_ExpDateStr
        public int PallID { get; set; }
        public char ItemType { get; set; }

    }
    public class PKL
    {
        public List<PKL1Dtls> PKL1Details { get; set; }
        public List<PKL2Dtls> PKL2Details { get; set; }
    }
    public class PKL1Dtls
    {
        public int AbsEntry { get; set; }
        public int PickEntry { get; set; }
        public int OrderEntry { get; set; }
        public int OrderLine { get; set; }
        public decimal PickQtty { get; set; }
        public char PickStatus { get; set; }
        public decimal RelQtty { get; set; }
        public int PrevReleas { get; set; }
        public int BaseObject { get; set; }
    }
    public class PKL2Dtls
    {
        public int AbsEntry { get; set; }
        public int PickEntry { get; set; }
        public int Pkl2LinNum { get; set; }
        public string ItemCode { get; set; }
        public char ManagedBy { get; set; }
        public int SnBEntry { get; set; }
        public int BinAbs { get; set; }
        public char AllowNeg { get; set; }
        public decimal RelQtty { get; set; }
        public decimal PickQtty { get; set; }
        public string objType { get; set; }

    }
    public class Integration
    {
        public char IntegrationChecked { get; set; }
        public string DBServerType { get; set; }
        public string DBServerIP { get; set; }
        public string DBUserName { get; set; }
        public string DBPassword { get; set; }
        public string SAPComDB { get; set; }
        public string SAPUserName { get; set; }
        public string SAPPassword { get; set; }
        public string SAPLicServer { get; set; }
    }

    public class DeliveryListDetails
    {
        public string BoxName { get; set; }
        public string BoxStatus { get; set; }
        public string Quantity { get; set; }

        public string CustName { get; set; }
        public string CustCode { get; set; }

        public string ItemName { get; set; }

        public string ItemDescription { get; set; }
    }

    public class AvailableQuantityToPick
    {
        public string Batch { get; set; }
        public int AllocatedQuantity { get; set; }
        public int PickedQuantity { get; set; }
        public int AvailableQuantity { get; set; }
    }

    public class PickModel
    {
        public string PickId { get; set; }
        public string Remarks { get; set; }
    }

    public class AllocationDetails
    {
        public string Batch { get; set; }
        public int AbsEntry { get; set; }
        public string ItemCode { get; set; }
        public int SystemNumber { get; set; }
        public int LineNumber { get; set; }
        public decimal AllocatedQuantity { get; set; }
    }

    public class PrintBoxDetail
    {
        public List<string> BoxIds { get; set; }
        public string UserName { get; set; }
        public int NoOfCopies { get; set; }
        public int PoOrderNumber { get; set; }
    }
}