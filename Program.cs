using Syncfusion.Drawing;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Security;
using System.Text.Json;
using SecurePdfGeneration;
using System.Text;
using Syncfusion.Licensing;

Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JEaF5cXmRCdkx3Qnxbf1x1ZFZMYFpbQXZPMyBoS35Rc0VkWHdec3RTQ2RfVEF3VEFd");
/// Loads and deserializes the bank statement data from a JSON file.
string jsonPath = "UserAccountDetails.json";
string jsonContent = File.ReadAllText(jsonPath);
StatementBuilder statement = JsonSerializer.Deserialize<StatementBuilder>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

// Load HTML template
string templatePath = "bank-statement-template.html";
string htmlTemplate = File.ReadAllText(templatePath);

// Replace top-level placeholders with actual data.
string filledHtmlContent = htmlTemplate
.Replace("{{statement.issue_date}}", statement.Statement.IssueDate)
.Replace("{{statement.period_start}}", statement.Statement.PeriodStart)
.Replace("{{statement.period_end}}", statement.Statement.PeriodEnd)
.Replace("{{bank.name}}", statement.Bank.Name)
.Replace("{{bank.account_number}}", statement.Statement.AccountNumber)
.Replace("{{bank.address}}", statement.Bank.Address)
.Replace("{{bank.customername}}", statement.Statement.CustomerName);


/// Builds the HTML table rows for each transaction in the bank statement
/// and replaces the {{ transaction_rows }} placeholder in the HTML template.
StringBuilder transactionRows = new StringBuilder();
foreach (var txn in statement.Transactions)
{
    transactionRows.AppendLine($@"
        <tr>
            <td>{txn.Date}</td>
            <td>{txn.Type}</td>
            <td>{txn.Detail}</td>
            <td>{txn.Debited}</td>
            <td>{txn.Credited}</td>
            <td>{txn.Balance}</td>
        </tr>");
}
filledHtmlContent = filledHtmlContent.Replace("{{ transaction_rows }}", transactionRows.ToString());


/// Converts the filled HTML content into a PDF document using Syncfusion's HTML to PDF converter.
HtmlToPdfConverter htmlConverter = new HtmlToPdfConverter();
BlinkConverterSettings settings = new BlinkConverterSettings
{
    Scale = 0.7f
};

// Apply the settings to the converter 
htmlConverter.ConverterSettings = settings;

// Convert the filled HTML content into a PDF document 
PdfDocument document = htmlConverter.Convert(filledHtmlContent, Path.GetFullPath("."));

/// Sets PDF security by applying a user password and restricting permissions to printing only.
// Access the security settings of the PDF document
PdfSecurity security = document.Security;

// Retrieve customer name and date of birth from the statement
string customerName = statement.Statement.CustomerName;
string dateOfBirth = statement.Statement.DateOfBirth;

// Extract the first 4 characters of the customer name and convert to uppercase
string namePrefix = (customerName.Substring(0, Math.Min(4, customerName.Length))).ToUpper();

// Parse the date of birth and format it to get day and month as "ddMM"
DateTime dob = DateTime.Parse(dateOfBirth);
string dateAndMonth = dob.ToString("ddMM");

// Combine name prefix and date/month to form the user password
string userPassword = namePrefix + dateAndMonth;

// Set the owner password to retain full control over the PDF, including editing and security settings.
// Required to enforce user-level restrictions like printing-only access and prevent unauthorized changes.
security.OwnerPassword = "G2bank1234";

// Set the user password for the PDF document
security.UserPassword = userPassword;

// Restrict permissions to allow only printing
security.Permissions = PdfPermissionsFlags.Print;

//Save the PDF document
Directory.CreateDirectory("../../../Output");
string outputPath = $"../../../Output/BankStatement_" + statement.Statement.CustomerName + "_" + statement.Statement.IssueDate + ".pdf";
MemoryStream stream = new MemoryStream();
document.Save(stream);
File.WriteAllBytes(outputPath, stream.ToArray());

// Close the document to release resources
document.Close(true);

Console.WriteLine("Documents saved successfully!");
Console.WriteLine("The password to open the document is: " + userPassword);


