using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Novell.Directory.Ldap;
using Xunit;

namespace Novell.Directory.LDAP.Tests
{
    public class ConnectionClassTest
    {
        [Fact]
        public void Connection_Reader_Thread_Should_Create_Cancelation_Token_Source()
        {
            var ldapConnection = new LdapConnection();
            var connectionProperty = ldapConnection.GetType().GetProperty("Connection", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var connection = connectionProperty.GetValue(ldapConnection);
            var readerThreadType = connection.GetType().GetNestedType("ReaderThread", BindingFlags.Public | BindingFlags.Instance);
            var readerThread = Activator.CreateInstance(readerThreadType, new object[] { connection });
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var cancellationTokenSourceField = readerThread.GetType().GetField("cancellationTokenSource", bindingFlags);
            var cancellationTokenSource = cancellationTokenSourceField.GetValue(readerThread);

            Assert.NotNull(cancellationTokenSource);
        }

        [Fact]
        public void Connection_Reader_Thread_Should_Create_Reader_Task_Field()
        {
            var ldapConnection = new LdapConnection();
            var connectionProperty = ldapConnection.GetType().GetProperty("Connection", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var connection = connectionProperty.GetValue(ldapConnection);
            var readerThreadType = connection.GetType().GetNestedType("ReaderThread", BindingFlags.Public | BindingFlags.Instance);
            var readerThread = Activator.CreateInstance(readerThreadType, new object[] { connection });
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var readerTaskField = readerThread.GetType().GetField("readerTask", bindingFlags);
            var readerTask = readerTaskField.GetValue(readerThread);

            Assert.NotNull(readerTask);
        }

        [Fact]
        public void Connection_Reader_Thread_Should_Start()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                  BindingFlags.Static;

            var ldapConnection = new LdapConnection();
            var connectionProperty = ldapConnection.GetType().GetProperty("Connection", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var connection = connectionProperty.GetValue(ldapConnection);
            var readerThreadType = connection.GetType().GetNestedType("ReaderThread", BindingFlags.Public | BindingFlags.Instance);
            var readerThread = Activator.CreateInstance(readerThreadType, new object[] { connection });

            var readerTaskField = readerThread.GetType().GetField("readerTask", bindingFlags);
            var readerTask = readerTaskField.GetValue(readerThread);

            var startMethod = readerThread.GetType().GetMethod("Start", bindingFlags);
            startMethod.Invoke(obj: readerThread, parameters: null);

            var taskStatusPropertyInfo = readerTask.GetType().GetProperty("Status");
            var taskStatus = taskStatusPropertyInfo.GetValue(readerTask);

            Assert.True((TaskStatus)taskStatus == TaskStatus.WaitingToRun || (TaskStatus)taskStatus == TaskStatus.Running);
        }

        [Fact]
        public void Connection_Reader_Thread_Should_Stop()
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                  BindingFlags.Static;

            var ldapConnection = new LdapConnection();
            var connectionProperty = ldapConnection.GetType().GetProperty("Connection", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var connection = connectionProperty.GetValue(ldapConnection);
            var readerThreadType = connection.GetType().GetNestedType("ReaderThread", BindingFlags.Public | BindingFlags.Instance);
            var readerThread = Activator.CreateInstance(readerThreadType, new object[] { connection });

            var readerTaskField = readerThread.GetType().GetField("readerTask", bindingFlags);
            var readerTask = readerTaskField.GetValue(readerThread);

            var startMethod = readerThread.GetType().GetMethod("Start", bindingFlags);
            startMethod.Invoke(obj: readerThread, parameters: null);


            var stopMethod = readerThread.GetType().GetMethod("Stop", bindingFlags);
            stopMethod.Invoke(obj: readerThread, parameters: null);
            Thread.Sleep(2000);
            var taskStatusPropertyInfo = readerTask.GetType().GetProperty("Status");
            var taskStatus = taskStatusPropertyInfo.GetValue(readerTask);

            Assert.True((TaskStatus)taskStatus == TaskStatus.Canceled || (TaskStatus)taskStatus == TaskStatus.RanToCompletion);
        }

        [Fact]
        public void Connection_Get_Problem_Message_Should_Be_Valid()
        {
            var ldapConnection = new LdapConnection();
            var connectionProperty = ldapConnection.GetType().GetProperty("Connection", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var connection = connectionProperty.GetValue(ldapConnection);

            MethodInfo GetProblemMessage = connection.GetType().GetMethod("GetProblemMessage", BindingFlags.NonPublic | BindingFlags.Static);
            var CertEXPIRED = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B0101 });
            Assert.Equal("CertEXPIRED", CertEXPIRED);

            var CertVALIDITYPERIODNESTING = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B0102 });
            Assert.Equal("CertVALIDITYPERIODNESTING", CertVALIDITYPERIODNESTING);

            var CertROLE = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B0103 });
            Assert.Equal("CertROLE", CertROLE);

            var CertPATHLENCONST = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B0104 });
            Assert.Equal("CertPATHLENCONST", CertPATHLENCONST);

            var CertCRITICAL = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B0105 });
            Assert.Equal("CertCRITICAL", CertCRITICAL);

            var CertPURPOSE = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B0106 });
            Assert.Equal("CertPURPOSE", CertPURPOSE);

            var CertISSUERCHAINING = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B0107 });
            Assert.Equal("CertISSUERCHAINING", CertISSUERCHAINING);

            var CertMALFORMED = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B0108 });
            Assert.Equal("CertMALFORMED", CertMALFORMED);

            var CertUNTRUSTEDROOT = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B0109 });
            Assert.Equal("CertUNTRUSTEDROOT", CertUNTRUSTEDROOT);

            var CertCHAINING = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B010A });
            Assert.Equal("CertCHAINING", CertCHAINING);

            var CertREVOKED = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B010C });
            Assert.Equal("CertREVOKED", CertREVOKED);

            var CertUNTRUSTEDTESTROOT = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B010D });
            Assert.Equal("CertUNTRUSTEDTESTROOT", CertUNTRUSTEDTESTROOT);

            var CertREVOCATION_FAILURE = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B010E });
            Assert.Equal("CertREVOCATION_FAILURE", CertREVOCATION_FAILURE);

            var CertCN_NO_MATCH = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B010F });
            Assert.Equal("CertCN_NO_MATCH", CertCN_NO_MATCH);

            var CertWRONG_USAGE = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B0110 });
            Assert.Equal("CertWRONG_USAGE", CertWRONG_USAGE);

            var CertUNTRUSTEDCA = GetProblemMessage.Invoke(obj: null, parameters: new object[] { 0x800B0112 });
            Assert.Equal("CertUNTRUSTEDCA", CertUNTRUSTEDCA);
        }

        [Fact]
        public void Connection_Copy_Method_Should_Return_Another_Instance_Of_Object()
        {
            var ldapConnection = new LdapConnection();
            var connectionProperty = ldapConnection.GetType().GetProperty("Connection", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var connection = connectionProperty.GetValue(ldapConnection);
            var CopyMethod = connection.GetType().GetMethod("copy", BindingFlags.NonPublic | BindingFlags.Instance);

            var connection1 = Activator.CreateInstance(connection.GetType(), true);
            var connection2 = CopyMethod.Invoke(obj: connection1, parameters: null);

            Assert.NotEqual(connection1, connection2);
        }
    }
}
