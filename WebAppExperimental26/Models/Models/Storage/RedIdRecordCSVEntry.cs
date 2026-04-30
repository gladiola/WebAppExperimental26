using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REDRFID.Models.Storage
{

    /*
 * 
 * {
    "id": "1",
    "FacilityCode": "A123",
    "CardNumber": "34125",
    "ParityBits": "8",
    "LocationAddress": "1144 Taylor St",
    "LocationCity": "New York",
    "LocationState": "NY",
    "LocationZip": "11580",
    "LocationLat": "41.40338",
    "LocationLong": "2.17403",
    "UploadedUser": "azaso",
    "UserID": "AndrewZaso",
    "RfidRisk": "Medium",
    "CompanyName": "IBM",
    "IdString":"asdfasdfasdf",
    "_partitionKey" : "85F05E2D1492C89F3EC1052A"

*/

    public record RedIdRecordCSVEntry
    {


        public string FacilityCode { get; set; } = String.Empty;
        public string CardNumber { get; set; } = String.Empty;

        // Should be server provided, not user
        public string ParityBits { get; set; } = String.Empty;
        public string LocationAddress { get; set; } = String.Empty;
        public string LocationCity { get; set; } = String.Empty;
        public string LocationState { get; set; } = String.Empty;
        public string LocationZip { get; set; } = String.Empty;
        public string LocationLat { get; set; } = String.Empty;
        public string LocationLong { get; set; } = String.Empty;
        public string RfidRisk { get; set;} = String.Empty;
        public string CompanyName { get; set;} = String.Empty;

        // Minimum requirement
        public string RFIDContent { get; set; } = String.Empty;


    }
}
