using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk.Query;

namespace MSCRMWorkflowRunner
{
    /// <summary>
    /// This code is supplied as is without guarantee. The latest code can be download from https://mscrmworkflowrunner.codeplex.com/
    /// </summary>
    class Program
    {
        private static string logfilename;

        static void Main(string[] args)
        {
            try
            {
                logfilename = string.Format("{0}.log", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                log("BEGIN");
                log("This code is supplied as is without guarantee. The latest code can be download from https://mscrmworkflowrunner.codeplex.com/");
                if (args.Count() != 1)
                {
                    throw new Exception("Invalid argument: config xml file.");
                }

                log(string.Format("Config file: {0}", args[0].ToString()));

                var configxml = new XmlDocument();
                configxml.Load(args[0].ToString());
                var connectionstring = configxml.SelectSingleNode("//config/connectionstring").InnerText;
                var workflownode = configxml.SelectSingleNode("//config/workflow");
                var workflowname = workflownode.Attributes["name"] != null ? workflownode.Attributes["name"].Value : null;
                Guid workflowid = workflownode.Attributes["id"] != null && !string.IsNullOrEmpty(workflownode.Attributes["id"].Value) ? new Guid(workflownode.Attributes["id"].Value) : Guid.Empty;
                var fetchxml = configxml.SelectSingleNode("//config/fetchxml").InnerText;

                var connection = CrmConnection.Parse(connectionstring);

                var service = new OrganizationService(connection);
                var context = new CrmOrganizationServiceContext(connection);

                if (workflowid == Guid.Empty)
                {
                    if (workflowname == null)
                    {
                        throw new Exception("Workflow name is required when no workflow id is specified!");
                    }
                    var query = new FetchExpression("<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>" +
                        "  <entity name='workflow'>" +
                        "    <attribute name='workflowid' />" +
                        "    <attribute name='name' />" +
                        "    <filter type='and'>" +
                        "      <condition attribute='category' operator='eq' value='0' />" +
                        "      <condition attribute='type' operator='eq' value='1' />" +
                        "      <condition attribute='name' operator='eq' value='" + workflowname + "' />" +
                        "    </filter>" +
                        "  </entity>" +
                        "</fetch>");
                    var workflows = service.RetrieveMultiple(query);
                    if (workflows.Entities.Count < 1)
                    {
                        throw new Exception(string.Format("A workflow with the name {0} could not be found!", workflowname));
                    }
                    else if (workflows.Entities.Count > 1)
                    {
                        throw new Exception(string.Format("More than one workflow with the name {0} found!", workflowname));
                    }
                    workflowid = workflows.Entities[0].Id;
                }

                var results = service.RetrieveMultiple(new FetchExpression(fetchxml));

                foreach (var entity in results.Entities)
                {
                    var req = new ExecuteWorkflowRequest()
                    {
                        EntityId = entity.Id,
                        WorkflowId = workflowid
                    };
                    try
                    {
                        service.Execute(req);
                        logsuccess(string.Format("Workflow request complete for entity id {0}", entity.Id.ToString()));
                    }
                    catch (Exception ex)
                    {
                        logerror(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                logerror(ex.Message);
            }
            finally
            {
                log("END");
            }
        }

        private static void logerror(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
            logtofile(string.Format("{0} - {1}", "ERROR", message));
        }

        private static void logsuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
            logtofile(string.Format("{0} - {1}", "SUCCESS", message));
        }

        private static void log(string message)
        {
            Console.WriteLine(message);
            logtofile(string.Format("{0} - {1}", "INFO", message));
        }

        private static void logtofile(string message)
        {
            try
            {
                File.AppendAllText(logfilename, string.Format("{0}: {1}{2}", DateTime.Now, message, Environment.NewLine));
            }
            catch { }
        }
    }
}
