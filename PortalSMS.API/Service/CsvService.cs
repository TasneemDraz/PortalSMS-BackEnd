using CsvHelper.Configuration;
using CsvHelper;
using PortalSMS.DAL.Data.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PortalSMS.API.Service
{
    public class CsvService
    {
        public IEnumerable<PhoneNumberRecord> ProcessCsv(Stream fileStream)
        {
            using (var reader = new StreamReader(fileStream))
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false, // No header
                    Delimiter = ",", // Comma-separated values
                };

                using (var csv = new CsvReader(reader, csvConfig))
                {
                    var records = new List<PhoneNumberRecord>();

                    while (csv.Read())
                    {
                        var row = csv.GetField<string>(0); // Get the entire row as a string
                        var phoneNumbers = row.Split(',');

                        foreach (var phoneNumber in phoneNumbers)
                        {
                            if (!string.IsNullOrWhiteSpace(phoneNumber))
                            {
                                var record = new PhoneNumberRecord
                                {
                                    PhoneNumber = phoneNumber.Trim()
                                };
                                records.Add(record);
                            }
                        }
                    }

                    return ValidatePhoneNumbers(records);
                }
            }
        }

        private IEnumerable<PhoneNumberRecord> ValidatePhoneNumbers(IEnumerable<PhoneNumberRecord> records)
        {
            return records.Where(record =>
                !string.IsNullOrEmpty(record.PhoneNumber) &&
                IsValidPhoneNumber(record.PhoneNumber)
            ).ToList();
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Check if the phone number starts with +20 and contains exactly 10 digits after it
            var regex = new Regex(@"^\+20\d{10}$");
            return regex.IsMatch(phoneNumber);
        }
    }


}
