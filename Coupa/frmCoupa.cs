using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Coupa
{
    public partial class frmCoupa : Form
    {
        public frmCoupa()
        {
            InitializeComponent();
        }

        private void frmCoupa_Load(object sender, EventArgs e)
        {
            string sHeader = "<InvoiceDetailRequest>" + GetXmlHeader() + "</InvoiceDetailRequest>";
            string sXml = "<Request>" + sHeader + GetXmlData("2011-12-31", "30101") + "</Request>";
            //string sDat = DateTime.Now.ToString("CCyyMMddHHmmss");
        }

        private string GetXmlHeader()
        {
            //timestamp = DateTime.UtcNow.ToString("s") + "-07:00"  payloadID = DateTime.Now.ToString("MMMddyyyy_HHmmtt")
            string sXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><!DOCTYPE cXML SYSTEM \"http://xml.cXML.org/schemas/cXML/1.2.020/InvoiceDetail.dtd\"><cXML timestamp=\"@timestamp\" payloadID=\"@payloadID\"><Header>";
            string sFrom = "<From><Credential domain=\"@domain\"><Identity>@IdentityFrom</Identity></Credential></From>";
            string sTo = "<To><Credential domain=\"@domain\"><Identity>@IdentityTo</Identity></Credential></To>";
            string sSec = "<Sender><Credential domain=\"Dom3\"><Identity>Test</Identity><SharedSecret>Secret</SharedSecret></Credential><UserAgent>Your Very Own Agent 1.23</UserAgent></Sender>";
            sXml += sFrom + sTo + sSec + "</Header>";
            return sXml;
        }

        //GetXml("2011-12-31", "30101");
        private string GetXmlData(string sDate, string sDeptID)
        {
            string sQuery = GetQuery("invoices").Replace("@date", sDate).Replace("@deptID", sDeptID);
            string sXml = GetData(sQuery, "CoupaConnString").Rows[0][0].ToString();
            return sXml;
        }

        //postXMLData("https://staples-test.coupahost.com/cxml/invoices", "<to do>");
        private string postXMLData(string destinationUrl, string requestXml)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
            byte[] bytes;
            bytes = System.Text.Encoding.ASCII.GetBytes(requestXml);
            request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;
            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();
                return responseStr;
            }
            return null;
        }

        private System.Data.DataTable GetData(string sQuery, string sBaza = "CoupaConnString")
        {
            string sConn = System.Configuration.ConfigurationManager.ConnectionStrings[sBaza].ConnectionString + ";Connect Timeout=180;";
            SqlConnection conn = new SqlConnection(sConn);

            conn.Open();
            SqlDataAdapter da = new SqlDataAdapter(sQuery, conn);
            DataSet dsCsv = new DataSet();
            da.Fill(dsCsv);
            conn.Close();

            return dsCsv.Tables[0];
        }

        private string GetQuery(string sOpt)
        {
            string sRes = "";
            switch (sOpt)
            {
                case "invoices":
                    sRes = @"
	                SELECT top 3
		                CONVERT(INT, h.SubDept) AS SUB_DEPT
		                ,SUBDNAME AS Site_Name
		                ,subdeptID AS SubDept_ID
		                ,PONUM AS PO_Number
		                ,[Work Date] AS Work_Date
		                ,ISNULL([TRLR CHRG],0) AS Load_Cost
		                ,ISNULL(SUM([TOTREV]),0) AS Total_Revenue
		                ,Payment AS Payment_Type_ID
		                ,[VENCD] AS Vendor
		                ,h.[LDTYPE] AS Load_Type_ID
		                ,[LDTYPEDESC] AS Load_Type
		                ,h.[UNLDTYP] AS Unload_Type_ID
		                ,[UNLDDESC] AS Unload_Type
		                ,[Dock]
		                ,h.DID AS Dock_ID
		                ,ISNULL(Pallets, 0) AS Initial_Pallets
		                ,ISNULL([PALLETBR], 0) AS Finished_Pallets
		                ,inv.INV AS Invoice_ID
		                ,h.rebill AS IsRebill
		                ,h.Remark
		                ,h.[INV DATE] as WeekEndDate
		                ,h.CUSTID as Bill_Code
		                ,h.BillRT
		                ,ISNULL(h.Processing_Fee, 0 ) as Processing_Fee
		                ,ISNULL(h.[TRLR CHRG], 0) - ISNULL(h.Processing_Fee, 0) as Load_Cost_Less_Processing_Fee                    
	                FROM [History] h
		                JOIN dbo.DockTAB dock ON h.DID = dock.DID
		                JOIN dbo.LDTYPETAB l ON h.[LDTYPE] = l.[LDTYPE]
		                JOIN dbo.UNLDTYP u ON h.[UNLDTYP] = u.[UNLDTYP]
		                JOIN dbo.SUBTAB s ON h.[SUBDEPT] = s.SUBDEPT
		                LEFT JOIN dbo.tblCommts com ON h.Comm2 = com.ComID
		                JOIN dbo.INVOICE inv ON h.INVNUM = inv.INVNUM
			                AND h.[INV DATE] = inv.[INV DATE]
	                WHERE h.[CANCEL] = 0
		                AND h.APPRO = 1
		                and h.[INV DATE] = '@date' /*between '2011-12-31' and '2011-12-31'--@begindate and @enddate*/
		                and s.subdeptID = @deptID /* '30101'COALESCE(@subdept, s.subdeptID)*/
		                AND h.[REBILL]  != 1
	                GROUP BY h.SubDept
		                ,SUBDNAME
		                ,subdeptID
		                ,PONUM
		                ,[Work Date]
		                ,[TRLR CHRG]
		                ,Payment
		                ,[VENCD]
		                ,h.[LDTYPE]
		                ,[LDTYPEDESC]
		                ,h.[UNLDTYP]
		                ,[UNLDDESC]
		                ,[Dock]
		                ,h.DID
		                ,Pallets
		                ,[PALLETBR]
		                ,inv.INV
		                ,h.rebill
		                ,h.Remark
		                ,h.[INV DATE]
		                ,h.CUSTID
		                ,h.BillRT
		                ,h.Processing_Fee
                    FOR XML PATH ('Invoice')
                    ";
                    break;
            }

            return sRes;
        }

        private float Ex(string sExpr)
        {
            string sOpers = "+-*/%|&^", sNumbers = "1234567890.,eE", sFuncs = "sin;cos;tan;asin;acos;atan;exp;ln;abs;sign;sqrt";
            
            return 0f;
        }


    }
}
