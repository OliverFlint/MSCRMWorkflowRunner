# MSCRMWorkflowRunner

A simple console app to execute on demand workflows either manually or via the windows task scheduler

**So how do I use it?**

The application takes one argument... The path to an xml file containing some configuration information.

This configuration consists of 3 elements:
* The connection string
* The workflow name and/or id
* A fetchxml query

**Sample Config**

    <config>
    	<connectionstring>Url=http://hostname/OrgName; Domain=mydomain; Username=Administrator; Password=Password1;</connectionstring>
    	<workflow name="Send Contact Birthday Email" id="" />
    	<fetchxml>
    		<![CDATA[
        <fetch version="1.0" output-format="xml-platform" mapping="logical" distinct="false">
          <entity name="contact">
            <attribute name="contactid" />
          </entity>
        </fetch>
    		]]>
    	</fetchxml>
    </config>

**The connection string**
The connection string is a standard mscrm connection string. Details of these can be found here http://msdn.microsoft.com/en-gb/library/gg695810.aspx

**The workflow name and/or id**
This element requires one or the other to be present. The name as you see it in mscrm or the id (GUID). The workflow must be enabled for "on demand" execution.

**A fetchxml query**
This is the query for the entities/records you wish to run the workflow against. The workflow will only be run against the entities/records return by this query.
