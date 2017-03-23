#region License
// /*******************************************************************************                                                                                                                                    
//  * Educational Online Test Delivery System                                                                                                                                                                       
//  * Copyright (c) 2014 American Institutes for Research                                                                                                                                                              
//  *                                                                                                                                                                                                                  
//  * Distributed under the AIR Open Source License, Version 1.0                                                                                                                                                       
//  * See accompanying file AIR-License-1_0.txt or at                                                                                                                                                                  
//  * http://www.smarterapp.org/documents/American_Institutes_for_Research_Open_Source_Software_License.pdf                                                                                                                                                 
//  ******************************************************************************/ 
#endregion
using Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TSS.Domain;
using TSS.Domain.DataModel;

namespace TSS.Services
{
    public class TestImportService: ITestImportService
    {
        private ILogger _logger = new Logger();
        public TestImportService()
        {
        }

        public School PopulateSchoolFromTdsReport(TDSReport tdsReport)
        {
            try
            {
                var school = new School();
                if (tdsReport.Examinee.Items != null)
                {
                    foreach (var obj in tdsReport.Examinee.Items.OfType<TDSReportExamineeExamineeRelationship>())
                    {
                        // if (obj.context != Context.FINAL) continue;
                        switch (obj.name)
                        {
                            case "SchoolID":
                                {
                                    school.SchoolID = obj.value;
                                    break;
                                }
                            case "SchoolName":
                                {
                                    school.SchoolName = obj.value;
                                    break;
                                }
                            case "StateName":
                                {
                                    school.StateName = obj.value;
                                    break;
                                }
                        }
                    }
                }

                if (string.IsNullOrEmpty(school.SchoolID) && string.IsNullOrEmpty(school.SchoolName) && string.IsNullOrEmpty(school.StateName))
                {
                    school.SchoolID = "0";
                    school.SchoolName = "unknown";
                    school.StateName = "unknown";
                }
                return school;
            }
            catch (Exception ex)
            {
                _logger.Error("Error Getting School Data from TDS Report", ex);
                throw new Exception("Error Populating School Data from TDS Report", ex);
            }
        }
        public District PopulateDistrictFromTdsReport(TDSReport tdsReport)
        {
            try
            {
                var district = new District();
                if (tdsReport.Examinee.Items != null)
                {
                    foreach (var obj in tdsReport.Examinee.Items.OfType<TDSReportExamineeExamineeRelationship>())
                    {
                        // if (obj.context != Context.FINAL) continue;
                        switch (obj.name)
                        {
                            case "DistrictID":
                                {
                                    district.DistrictID = obj.value;
                                    break;
                                }
                            case "DistrictName":
                                {
                                    district.DistrictName = obj.value;
                                    break;
                                }
                        }
                    }
                }

                if (string.IsNullOrEmpty(district.DistrictName) && string.IsNullOrEmpty(district.DistrictID))
                {
                    district.DistrictID = "0";
                    district.DistrictName = "unknown";
                }

                return district;
            }
            catch (Exception ee)
            {
                _logger.Error("Error Getting District Data from TDS Report", ee);
                throw new Exception("Error Populating District Data from TDS Report", ee);
            }
        }
        public Student PopulateStudentFromTdsReport(TDSReport tdsReport)
        {

            try
            {
                var student = new Student
                {
                    StudentId = tdsReport.Examinee.key,
                    Dob = DateTime.MaxValue
                };

                if (tdsReport.Examinee.Items != null && tdsReport.Examinee.Items.Count() > 0)
                {
                    foreach (var obj in tdsReport.Examinee.Items.OfType<TDSReportExamineeExamineeAttribute>())
                    {
                        // if (obj.context != Context.FINAL) continue;
                        switch (obj.name)
                        {
                            case "DOB":
                                {
                                    DateTime stuDob;
                                    if (DateTime.TryParseExact(obj.value, "MMddyyyy", null, DateTimeStyles.None, out stuDob))
                                    {
                                        student.Dob = stuDob;
                                    }
                                    break;
                                }
                            case "FirstName":
                                {
                                    student.FirstName = obj.value;
                                    break;
                                }
                            case "LastOrSurname":
                                {
                                    student.LastName = obj.value;
                                    break;
                                }
                            case "Grade":
                                {
                                    student.Grade = obj.value;
                                    break;
                                }
                            case "SSID":
                                {
                                    student.SSID = obj.value;
                                    break;
                                }
                            case "TDSLoginID":
                                {
                                    student.TdsLoginId = obj.value;
                                    break;
                                }
                            case "TDSTesteeName":
                                {
                                    student.Name = obj.value;
                                    break;
                                }
                        }
                    }
                }
                return student;
            }
            catch (Exception exec)
            {
                _logger.Error("Error Getting Student Data from TDS Report", exec);
                throw new Exception("Error Populating Student Data from TDS Report", exec);
            }
        }
        public Teacher PopulateTeacherFromTdsReport(TDSReport tdsReport)
        {
            try
            {
                var teacher = new Teacher
                {
                    Name = tdsReport.Opportunity.taName,
                    TeacherID = tdsReport.Opportunity.taId
                };
                return teacher;
            }
            catch (Exception exc)
            {
                _logger.Error("Error Getting Teacher Data from TDS Report", exc);
                throw new Exception("Error Populating Teacher Data from TDS Report", exc);
            }
        }
        public Test PopulateTestFromTdsReport(TDSReport tdsReport)
        {
            var test = new Test();
            try
            {

                test.AcademicYear = (int)tdsReport.Test.academicYear;
                test.AssessmentType = tdsReport.Test.assessmentType == null ? "TEST" : tdsReport.Test.assessmentType;
                test.Bank = (int)tdsReport.Test.bankKey;
                test.Contract = tdsReport.Test.contract;
                test.Grade = tdsReport.Test.grade;
                test.TestId = tdsReport.Test.testId;

                switch (tdsReport.Test.mode)
                {
                    case TDSReportTestMode.online:
                        {
                            test.Mode = "online";
                            break;
                        }
                    case TDSReportTestMode.paper:
                        {
                            test.Mode = "paper";
                            break;
                        }
                    default:
                        {
                            test.Mode = "scanned";
                            break;
                        }
                }

                test.Name = tdsReport.Test.name;
                test.Subject = tdsReport.Test.subject;
                test.Version = tdsReport.Test.assessmentVersion == null ? "1" : tdsReport.Test.assessmentVersion;

                return test;
            }
            catch (Exception e)
            {
                _logger.Error("Error Getting Test Data from TDS Report", e);
                throw new Exception("Error Populating Test Data from TDS Report", e);
            }
        }

        // IRP Items
        private static List<ItemType> ITEM_TYPES = new List<ItemType>
        {
            // Hand score items
            new ItemType { BankKey = 187, ItemKey = 1432 },
            new ItemType { BankKey = 187, ItemKey = 2061 },
            new ItemType { BankKey = 187, ItemKey = 2063 },
            new ItemType { BankKey = 187, ItemKey = 2129 },
            new ItemType { BankKey = 187, ItemKey = 2458 },
            new ItemType { BankKey = 187, ItemKey = 2491 },
            new ItemType { BankKey = 187, ItemKey = 2492 },
            new ItemType { BankKey = 187, ItemKey = 2493 },
            new ItemType { BankKey = 187, ItemKey = 2558 },
            new ItemType { BankKey = 187, ItemKey = 2595 },
            new ItemType { BankKey = 187, ItemKey = 2615 },
            new ItemType { BankKey = 187, ItemKey = 2616 },
            new ItemType { BankKey = 187, ItemKey = 2630 },
            new ItemType { BankKey = 187, ItemKey = 2635 },
            new ItemType { BankKey = 187, ItemKey = 2636 },
            new ItemType { BankKey = 187, ItemKey = 2638 },
            new ItemType { BankKey = 187, ItemKey = 2644 },
            new ItemType { BankKey = 187, ItemKey = 2645 },
            new ItemType { BankKey = 187, ItemKey = 2684 },
            new ItemType { BankKey = 187, ItemKey = 2700 },
            new ItemType { BankKey = 187, ItemKey = 2701 },
            new ItemType { BankKey = 187, ItemKey = 2703 },
            new ItemType { BankKey = 187, ItemKey = 2704 },
            new ItemType { BankKey = 187, ItemKey = 2709 },
            // Machine scored types are added here just in case they need a score
            new ItemType { BankKey = 187, ItemKey = 1434 },
            new ItemType { BankKey = 187, ItemKey = 1793 },
            new ItemType { BankKey = 187, ItemKey = 1825 },
            new ItemType { BankKey = 187, ItemKey = 1827 },
            new ItemType { BankKey = 187, ItemKey = 1832 },
            new ItemType { BankKey = 187, ItemKey = 1835 },
            new ItemType { BankKey = 187, ItemKey = 1838 },
            new ItemType { BankKey = 187, ItemKey = 1839 },
            new ItemType { BankKey = 187, ItemKey = 1840 },
            new ItemType { BankKey = 187, ItemKey = 1842 },
            new ItemType { BankKey = 187, ItemKey = 1844 },
            new ItemType { BankKey = 187, ItemKey = 1873 },
            new ItemType { BankKey = 187, ItemKey = 1876 },
            new ItemType { BankKey = 187, ItemKey = 1881 },
            new ItemType { BankKey = 187, ItemKey = 1882 },
            new ItemType { BankKey = 187, ItemKey = 1883 },
            new ItemType { BankKey = 187, ItemKey = 1889 },
            new ItemType { BankKey = 187, ItemKey = 1899 },
            new ItemType { BankKey = 187, ItemKey = 1915 },
            new ItemType { BankKey = 187, ItemKey = 1916 },
            new ItemType { BankKey = 187, ItemKey = 1926 },
            new ItemType { BankKey = 187, ItemKey = 1929 },
            new ItemType { BankKey = 187, ItemKey = 1930 },
            new ItemType { BankKey = 187, ItemKey = 1936 },
            new ItemType { BankKey = 187, ItemKey = 1948 },
            new ItemType { BankKey = 187, ItemKey = 1956 },
            new ItemType { BankKey = 187, ItemKey = 1966 },
            new ItemType { BankKey = 187, ItemKey = 1969 },
            new ItemType { BankKey = 187, ItemKey = 1972 },
            new ItemType { BankKey = 187, ItemKey = 1973 },
            new ItemType { BankKey = 187, ItemKey = 1975 },
            new ItemType { BankKey = 187, ItemKey = 1976 },
            new ItemType { BankKey = 187, ItemKey = 1978 },
            new ItemType { BankKey = 187, ItemKey = 1979 },
            new ItemType { BankKey = 187, ItemKey = 1982 },
            new ItemType { BankKey = 187, ItemKey = 1983 },
            new ItemType { BankKey = 187, ItemKey = 1986 },
            new ItemType { BankKey = 187, ItemKey = 1987 },
            new ItemType { BankKey = 187, ItemKey = 1988 },
            new ItemType { BankKey = 187, ItemKey = 1991 },
            new ItemType { BankKey = 187, ItemKey = 1996 },
            new ItemType { BankKey = 187, ItemKey = 1997 },
            new ItemType { BankKey = 187, ItemKey = 1998 },
            new ItemType { BankKey = 187, ItemKey = 1999 },
            new ItemType { BankKey = 187, ItemKey = 2001 },
            new ItemType { BankKey = 187, ItemKey = 2005 },
            new ItemType { BankKey = 187, ItemKey = 2006 },
            new ItemType { BankKey = 187, ItemKey = 2015 },
            new ItemType { BankKey = 187, ItemKey = 2017 },
            new ItemType { BankKey = 187, ItemKey = 2024 },
            new ItemType { BankKey = 187, ItemKey = 2029 },
            new ItemType { BankKey = 187, ItemKey = 2032 },
            new ItemType { BankKey = 187, ItemKey = 2059 },
            new ItemType { BankKey = 187, ItemKey = 2060 },
            new ItemType { BankKey = 187, ItemKey = 2118 },
            new ItemType { BankKey = 187, ItemKey = 2129 },
            new ItemType { BankKey = 187, ItemKey = 2131 },
            new ItemType { BankKey = 187, ItemKey = 2454 },
            new ItemType { BankKey = 187, ItemKey = 2458 },
            new ItemType { BankKey = 187, ItemKey = 2462 },
            new ItemType { BankKey = 187, ItemKey = 2463 },
            new ItemType { BankKey = 187, ItemKey = 2467 },
            new ItemType { BankKey = 187, ItemKey = 2472 },
            new ItemType { BankKey = 187, ItemKey = 2491 },
            new ItemType { BankKey = 187, ItemKey = 2492 },
            new ItemType { BankKey = 187, ItemKey = 2493 },
            new ItemType { BankKey = 187, ItemKey = 2497 },
            new ItemType { BankKey = 187, ItemKey = 2498 },
            new ItemType { BankKey = 187, ItemKey = 2500 },
            new ItemType { BankKey = 187, ItemKey = 2510 },
            new ItemType { BankKey = 187, ItemKey = 2521 },
            new ItemType { BankKey = 187, ItemKey = 2524 },
            new ItemType { BankKey = 187, ItemKey = 2526 },
            new ItemType { BankKey = 187, ItemKey = 2529 },
            new ItemType { BankKey = 187, ItemKey = 2561 },
            new ItemType { BankKey = 187, ItemKey = 2564 },
            new ItemType { BankKey = 187, ItemKey = 2565 },
            new ItemType { BankKey = 187, ItemKey = 2567 },
            new ItemType { BankKey = 187, ItemKey = 2570 },
            new ItemType { BankKey = 187, ItemKey = 2572 },
            new ItemType { BankKey = 187, ItemKey = 2576 },
            new ItemType { BankKey = 187, ItemKey = 2577 },
            new ItemType { BankKey = 187, ItemKey = 2578 },
            new ItemType { BankKey = 187, ItemKey = 2579 },
            new ItemType { BankKey = 187, ItemKey = 2590 },
            new ItemType { BankKey = 187, ItemKey = 2591 },
            new ItemType { BankKey = 187, ItemKey = 2594 },
            new ItemType { BankKey = 187, ItemKey = 2597 },
            new ItemType { BankKey = 187, ItemKey = 2601 },
            new ItemType { BankKey = 187, ItemKey = 2603 },
            new ItemType { BankKey = 187, ItemKey = 2604 },
            new ItemType { BankKey = 187, ItemKey = 2610 },
            new ItemType { BankKey = 187, ItemKey = 2616 },
            new ItemType { BankKey = 187, ItemKey = 2622 },
            new ItemType { BankKey = 187, ItemKey = 2624 },
            new ItemType { BankKey = 187, ItemKey = 2626 },
            new ItemType { BankKey = 187, ItemKey = 2627 },
            new ItemType { BankKey = 187, ItemKey = 2637 },
            new ItemType { BankKey = 187, ItemKey = 2643 },
            new ItemType { BankKey = 187, ItemKey = 2659 },
            new ItemType { BankKey = 187, ItemKey = 2660 },
            new ItemType { BankKey = 187, ItemKey = 2661 },
            new ItemType { BankKey = 187, ItemKey = 2684 },
            new ItemType { BankKey = 187, ItemKey = 2685 },
            new ItemType { BankKey = 187, ItemKey = 2702 },
            new ItemType { BankKey = 187, ItemKey = 2704 },
            new ItemType { BankKey = 187, ItemKey = 2705 },
            new ItemType { BankKey = 187, ItemKey = 2707 }
        };
        
        /// <summary>
        /// This is where we import all the things from TDS XML report.  
        /// There are a couple of assumptions we make here:
        /// 1.  The entire opportunity is imported at one time.  The dependency-checking logic expects this.
        /// 2.  The items are all in the same bank key.  This is always true in TDS tests.
        /// </summary>
        /// <param name="tdsReport"></param>
        /// <returns></returns>
        public List<StudentResponse> PopulateItemsFromTdsReport(TDSReport tdsReport)
        {
            try
            {
                List<ItemType> itemTypes = ITEM_TYPES;
                var responses = new List<StudentResponse>();

                // We need to include some responses that may be scored, but have dependent responses.
                Dictionary<int, StudentResponse> allResponses = new Dictionary<int, StudentResponse>();
                Dictionary<int, List<StudentResponse>> responseToPassageMap = new Dictionary<int, List<StudentResponse>>();

                foreach (var item in tdsReport.Opportunity.Item)
                {
                    //if an item hasn’t been configured in THSS – ignore the item.
                    ItemType currentItem = itemTypes.Find(ci => ci.ItemKey == item.key && ci.BankKey == item.bankKey);
                    if (currentItem == null)
                    {
                        continue;
                    }

                    // Magic number: db uses '2' to mean scored.  We use it later so record that here.
                    int score = (String.IsNullOrEmpty(item.scoreStatus) ||
                                 item.scoreStatus == TDSReportOpportunityItemScoreStatus.SCORED.ToString())
                                    ? 2
                                    : 0;

                    if (item.Response.Text == null) item.Response.Text = new string[0];
                    var studentResponse = new StudentResponse()
                    {
                        BankKey = (int)item.bankKey,
                        ContentLevel = item.contentLevel,
                        Format = item.format,
                        ItemKey = (int)item.key,
                        ResponseDate = item.Response.date,
                        Response = string.Join("", item.Response.Text),
                        SegmentId = item.segmentId,
                        ScoreStatus = score
                    };

                    allResponses.Add(studentResponse.ItemKey, studentResponse);

                    // If this item has a passage, record it.
                    if (currentItem.Passage == 0)
                    {
                        continue;
                    }
                    // If this item has a passage, add it to the list of items for that passage
                    bool passageExists = responseToPassageMap.ContainsKey(currentItem.Passage);
                    List<StudentResponse> passageResponses = (passageExists)
                                                                 ? responseToPassageMap[currentItem.Passage]
                                                                 : new List<StudentResponse>();

                    if (!passageExists)
                    {
                        responseToPassageMap.Add(currentItem.Passage, passageResponses);
                    }
                    passageResponses.Add(studentResponse);
                }

                IEnumerable<int> keys = allResponses.Keys;
                foreach (int key in keys)
                {
                    StudentResponse testResponse = allResponses[key];
                    ItemType currentItem =
                        itemTypes.Find(ci => ci.ItemKey == testResponse.ItemKey &&
                                             ci.BankKey == testResponse.BankKey);

                    //If the item is not already scored, we for sure want it.
                    if (testResponse.ScoreStatus != 2)
                    {
                        responses.Add(testResponse);
                        continue;
                    }

                    // If the item is scored, and it doesn't have a passage, we for sure don't want it.
                    if (!responseToPassageMap.ContainsKey(currentItem.Passage))
                    {
                        continue;
                    }

                    // if the item is scored, but has a passage, see if an unscored item has the same passage.
                    List<StudentResponse> passageResponses = responseToPassageMap[currentItem.Passage];
                    foreach (StudentResponse passageResponse in passageResponses)
                    {
                        if (passageResponse.ItemKey == testResponse.ItemKey ||
                            passageResponse.ScoreStatus == 2)
                        {
                            continue;
                        }
                        // An unscored item has the same passage.  We need this for scoring, so add it.
                        responses.Add(testResponse);
                        break;
                    }
                }
                return responses;
            }
            catch (Exception ex)
            {
                _logger.Error("Error Getting Item Data from TDS Report", ex);
                throw new Exception("Error Populating Item Data from TDS Report", ex);
            }
        }
    }
}
