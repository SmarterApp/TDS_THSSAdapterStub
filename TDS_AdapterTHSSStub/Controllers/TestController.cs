using Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using TDS_AdapterTHSSStub.Models;
using TSS.Domain;
using TSS.Domain.DataModel;
using TSS.Services;

namespace TDS_AdapterTHSSStub.Controllers
{
    public class TestController : Controller
    {
        private static ILogger _logger = new Logger();

        // Create a TestImportService to help get information from TDS Report.
        private ITestImportService _testImportService = new TestImportService();

        public ActionResult Hello()
        {
            var json = Json("hello");
            json.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            return json;
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Submit()
        {
            var apiResult = new TestSubmitApiResultModel();
            if (Request.Files.Count > 0)
            {
                // Store automatically scored responses for all file and hand-score items
                List<StudentResponseAssignment> scoredResponses = new List<StudentResponseAssignment>();

                foreach (string fileName in Request.Files)
                {
                    var fileResult = new TestSubmitApiResultFileModel();
                    apiResult.Files.Add(fileResult);

                    fileResult.Success = false;
                    try
                    {
                        HttpPostedFileBase file = Request.Files[fileName];
                        if (file != null && file.ContentLength > 0)
                        {
                            fileResult.FileName = file.FileName;
                            var binReader = new BinaryReader(file.InputStream);
                            var binData = binReader.ReadBytes((int)file.InputStream.Length);
                            var memoryStream = new MemoryStream(binData);
                            var streamReader = new StreamReader(memoryStream);


                            ////validate xml file
                            XmlDocument doc = new XmlDocument();
                            doc.Load(streamReader);
                            
                            _logger.Info(file.FileName + " : " + doc.OuterXml);

                            string xsdPath = Server.MapPath("~/bin/App_Data/reportxml_os.xsd");
                            string errorString = TSS.MVC.Helpers.SchemaHelper.Validate(xsdPath, doc);
                            string validationOutput = string.IsNullOrEmpty(errorString)
                                                            ? String.Empty
                                                            : " File is not in a correct format. Validation Error:" +
                                                            errorString;

                            if (string.IsNullOrEmpty(errorString))
                            {
                                var serializer = new XmlSerializer(typeof(ItemScoreRequest));
                                try
                                {
                                    memoryStream.Position = 0;
                                    streamReader.DiscardBufferedData();
                                    var itemScoreRequest = (ItemScoreRequest)serializer.Deserialize(streamReader);
                                    streamReader.Close();
                                    memoryStream.Close();
                                    binReader.Close();

                                    scoredResponses.AddRange(ProcessScoreRequest(itemScoreRequest));

                                    fileResult.Success = true;
                                }
                                catch (Exception ex)
                                {
                                    fileResult.ErrorMessage = "There was an error processing the file. " + ex.Message +
                                                                ex.StackTrace;
                                    fileResult.Success = false;
                                    
                                    _logger.Error(fileResult.ErrorMessage, ex);
                                }
                            }
                            else
                            {
                                // if validation fails, then log the validation error and proceed with next request
                                fileResult.ErrorMessage = validationOutput;
                                fileResult.Success = false;
                                
                                _logger.Info(fileResult.ErrorMessage);
                            }
                        }
                        else// end of file null or file content empty
                        {
                            fileResult.Success = false;
                            fileResult.ErrorMessage = "Error Code: 1002 Message: File does not contain any data.";
                            
                            _logger.Info(fileResult.ErrorMessage);
                        }

                    }
                    catch (Exception exp)
                    {
                        fileResult.ErrorMessage = exp.Message + exp.StackTrace;
                        fileResult.Success = false;
                        _logger.Error("Error", exp);
                    }
                }

                // Submit the scored responses asynchronously
                System.Threading.Timer timer = null;
                timer = new System.Threading.Timer((obj) =>
                {
                    // Create an ExportService to send scored results back to TIS.
                    IExportService _exportService = new ExportService(_logger);

                    foreach (var scoredResponse in scoredResponses)
                    {
                        _exportService.SendScoreReport(scoredResponse);
                    }

                    timer.Dispose();
                }, null, 1000, System.Threading.Timeout.Infinite);
            }
            
            return Json(apiResult);
        }

        private List<StudentResponseAssignment> ProcessScoreRequest(ItemScoreRequest itemScoreRequest)
        {
            List<StudentResponseAssignment> scoredResponses = new List<StudentResponseAssignment>();

            var tdsReport = itemScoreRequest.TDSReport;
            var district = _testImportService.PopulateDistrictFromTdsReport(tdsReport);
            var test = _testImportService.PopulateTestFromTdsReport(tdsReport);
            var teacher = _testImportService.PopulateTeacherFromTdsReport(tdsReport);
            var student = _testImportService.PopulateStudentFromTdsReport(tdsReport);

            // Very important: this returns hand-score item types
            var responses = _testImportService.PopulateItemsFromTdsReport(tdsReport);
            _logger.Info("Number of hand score items in TDS Report: " + responses.Count);

            // Automatically give each item a score of zero
            foreach (var response in responses)
            {
                StudentResponseAssignment assignment = new StudentResponseAssignment();
                assignment.Test = test;
                assignment.Teacher = teacher;
                assignment.Student = student;
                assignment.SessionId = tdsReport.Opportunity.sessionId;
                assignment.OpportunityId = long.Parse(tdsReport.Opportunity.oppId);
                assignment.OpportunityKey = Guid.Parse(tdsReport.Opportunity.key);
                assignment.ClientName = tdsReport.Opportunity.clientName;
                assignment.CallbackUrl = itemScoreRequest.callbackUrl;

                // Data from loop
                assignment.StudentResponse = new StudentResponse
                {
                    BankKey = response.BankKey,
                    ItemKey = response.ItemKey,
                    Format = response.Format
                };

                // Automatic score of 0
                assignment.ScoreData = "<score><dimension><score>0</score><conditioncode></conditioncode></dimension></score>";

                scoredResponses.Add(assignment);
            }

            return scoredResponses;
        }

        // api/test/successresponse - xml
        //[System.Web.Mvc.HttpPost]
        public ActionResult SuccessResponse(string xml)
        {
            return Json("OK", JsonRequestBehavior.AllowGet);
        }
    }
}