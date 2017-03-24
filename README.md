Automated Hand Scoring Stub for the SBAC Open Source Assessment Delivery System

Overview
--------

This web application can be used in place of the SBAC Teacher Hand Scoring Service
when using the IRP Automation Adapter to automate the process of generating
TDS Report XML documents.  When the IRP Automation Adapter is run against
a deployment of the SBAC Open Source Assessment Delivery System, the Test
Integration Service will need a component to score hand-scored items, such as
ER, WER, and SA, automatically.  Otherwise, TIS will not be able to 
generated fully scored TDS Report XML documents.  

This component/application was primarily developed for use with IRP 2.0
and the IRP Automation Adapter.  Therefore, this application has hard-coded
IRP Items into the TestImportService class.  See TestImportService.cs and look for
'ITEM_TYPES'.

This component will automatically score all items it receives with a score of zero. 

To change the score or to program custom scoring logic, look at TestController.cs for the 
method `ProcessScoreRequest` and the line `assignment.ScoreData`.  The XML 
string value saved into `assignment.ScoreData` contains the score. However, it's recommended to preserve the feature of scoring all items with a score of zero as 
IRP might expect those items to have those scores in the future.

Deployment/Installation
-----------------------

This project is a .NET 4.5 Web API application developed using Visual Studio 2015.
To use this application, use the Publish feature in Visual Studio to publish
the TDS_AdapterTHSSStub project to an IIS configured directory.  Or deploy it 
according to your own organization's .NET deployment strategy.

After deploying this application, it will process item scoring requests at the
URL:

`http://localhost/<path to application>/test/submit`

Where `<path to application>` represents the location you configured IIS to 
host this application.

Test Integration Service (TIS) Configuration
--------------------------------------------

TIS must be configured to submit hand-scored items to this application so those
items can be automatically scored.  Configure the TISService (aka OSS_TISService)
Windows Service component with the following suggested settings:

App.config (aka TISService.exe.config, TISService.vhost.exe.config)
```
#!xml
<WebServiceSettings>
  <WebService name="HandscoringTSS" url="http://localhost/adapter_thss_stub/test/submit" />
  <!-- The rest of the existing WebServiceSettings go here -->
</WebServiceSettings>
<!-- ... -->
<ItemScoringSettings>
  <ItemScoring target="HandscoringTSS"
	callbackUrl="http://localhost/oss_tis_itemscoring/ItemScoringCallbackRcv.axd"
	itemTypes="ER;SA;WER;WIT;EBSR;EQ;ETC;GI;HT;HTQ;MC;MI;MS;NL;TI"
	isHandscoringTarget="true"
	batchRequest="true"
	scoreStatuses="NotScored,WaitingForMachineScore,ScoringError"/>
  <ItemScoring target="HandscoringTDSUnscored"
	callbackUrl="http://localhost/oss_tis_itemscoring/ItemScoringCallbackRcv.axd"
	itemTypes=""
	scoreStatuses="WaitingForMachineScore" />
</ItemScoringSettings>
```

The above setting will configure 'HandscoringTSS' to point to this application.
All hand-score items will be submitted to this application.
All non-hand-score items that are waiting for a machine score or have scoring error
will be submitted to this application.

Restart TISService after making these changes and remember to revert them when
you are done.