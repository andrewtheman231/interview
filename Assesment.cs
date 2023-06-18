using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Data.OleDb;
namespace BillingSystem
{
    class Assesment
    {
        public static void Main(string[] args)
        {
            string xmlFile = "BillFile.xml";
            string outputFile = $"BillFile-{DateTime.Now.ToString("MMddyyyy")}.rpt";
            XDocument billFile =  XDocument.Load(xmlFile);
            XElement billData = billFile.Root;
            BillingOutput(outputFile, billData);
            ImportDataIntoMDBFile(outputFile);
            Console.WriteLine($"Output File {outputFile} created successfully");
        }
        static void BillingOutput(string outputFile, XElement billData)
        {
               using(StreamWriter writer = new StreamWriter(outputFile))
            {       string currentDateTime = DateTime.Now.ToString("MM/dd/yyyy");
                    int? invoiceRecordCount = billData.Descendants("BILL_HEADER")?.Count();
                    double? invoiceRecordTotalAmount = billData.Descendants("Bill_Amount")?.Sum(e => double.Parse(e.Value));
                    writer.WriteLine($"1~FR|2~8203ACC7-2094-43CC-8F7A-B8F19AA9BDA2|3~Sample UT file|4~{currentDateTime}|5~{invoiceRecordCount}|6~{invoiceRecordTotalAmount}");

                foreach (XElement billHeader in billData.Elements("BILL_HEADER"))
                {
                    
                    string invoiceNumber = billHeader.Element("Invoice_No")?.Value;
                    string accountNumber = billHeader.Element("Account_No")?.Value;
                    string customerName = billHeader.Element("Customer_Name")?.Value;
                    string billDate = billHeader.Element("Bill_Dt")?.Value;
                    string dueDate = billHeader.Element("Due_Dt")?.Value;
                    string billAmount = billHeader.Element("Bill")?.Element("Bill_Amount")?.Value;
                    string balanceDue = billHeader.Element("Bill")?.Element("Balance_Due")?.Value;
                    string address1 = billHeader.Element("Address_Information")?.Element("Mailing_Address_1")?.Value;
                    string address2 = billHeader.Element("Address_Information")?.Element("Mailing_Address_2")?.Value;
                    string city = billHeader.Element("Address_Information")?.Element("City")?.Value;
                    string state = billHeader.Element("Address_Information")?.Element("State")?.Value;
                    string zip = billHeader.Element("Address_Information")?.Element("Zip")?.Value;
                    
                  
                    writer.WriteLine($"AA~CT|BB~{accountNumber}|VV~{customerName}|CC~{address1}|DD~{address2}|EE~{city}|FF~{state}|GG~{zip}");
                    writer.WriteLine($"HH~IH|II~R|JJ~8E2FEA69-5D77-4D0F-898E-DFA25677D19E|KK~{invoiceNumber}|LL~{billDate}|MM~{dueDate}|NN~{billAmount}|OO~{DateTime.Now.AddDays(5):MM/dd/yyyy}|PP~{dueDate.Substring(0, 2)}|QQ~{balanceDue}|RR~{currentDateTime}|SS~{address1}");
                }
            }
        }
        static void ImportDataIntoMDBFile(string rptFile)
        {
            string connectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Billing.mdb;";

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                connection.Open();

                using (StreamReader reader = new StreamReader(rptFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // Parse the line and extract the data for insertion into the database
                        string[] fields = line.Split('|');
                        string fieldId = fields[0];
                        string fieldValue = fields[2];

                       string id = GetValueByKey(fields, "BB");
                       string billDate = GetValueByKey(fields, "LL");
                       string billNumber = GetValueByKey(fields, "KK");
                       string billAmount = GetValueByKey(fields, "NN");
                       string FormatGUID = GetValueByKey(fields, "JJ");
                       string accountBalance = GetValueByKey(fields, "QQ");
                       string dueDate = GetValueByKey(fields, "MM");
                       string serviceAddress = GetValueByKey(fields, "SS");
                       string firstEmailDate = GetValueByKey(fields, "OO");
                       string secondEmailDate = GetValueByKey(fields, "PP");
                       string dateAdded = GetValueByKey(fields, "RR");
                       string customerId = GetValueByKey(fields, "2");
                        OleDbCommand command = new OleDbCommand("INSERT INTO Bills (ID, BillDate, BillNumber, BillAmount, FormatGUID, AccountBalance, DueDate, ServiceAddress, FirstEmailDate, SecondEmailDate, DateAdded, CustomerId) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)", connection);
                        command.Parameters.AddWithValue("ID", id);
                        command.Parameters.AddWithValue("BillDate", billDate);
                        command.Parameters.AddWithValue("BillNumber", billNumber);
                        command.Parameters.AddWithValue("BillAmount", billAmount);
                        command.Parameters.AddWithValue("FormatGUID", FormatGUID);
                        command.Parameters.AddWithValue("AccountBalance", accountBalance);
                        command.Parameters.AddWithValue("DueDate", dueDate);
                        command.Parameters.AddWithValue("ServiceAddress", serviceAddress);
                        command.Parameters.AddWithValue("FirstEmailDate", firstEmailDate);
                        command.Parameters.AddWithValue("SecondEmailDate", secondEmailDate);
                        command.Parameters.AddWithValue("DateAdded", dateAdded);
                        command.Parameters.AddWithValue("CustomerId", customerId);
                        command.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }
        static string GetValueByKey(string[] fields, string key)
{
    foreach (string field in fields)
    {
        string[] keyValue = field.Split('|');
        if (keyValue.Length == 2 && keyValue[0] == key)
            return keyValue[2];
    }
    return null;
}
    }
}