/******************************************************************************
* The MIT License
* Copyright (c) 2014 VQ Communications Ltd.  www.vqcomms.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Samples.Controls.PGControl.cs
//
// Author:
//   Igor Shmukler
//
// (C) 2014 VQ Communications Ltd. (http://www.vqcomms.com)
//

using System;
using System.Text;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Controls;

/// <summary>  The following sample demonstrates how to use the 
/// Paging Results control with Synchronous search requests.
/// 
/// The program is hard coded to sort based on the common name
/// attribute, and it searches for all objects at the specified
/// searchBase.
/// 
/// Usage: Usage: PGControl <host name> <login dn> <password> 
/// <searchBase>
/// 
/// </summary>

public class PGControl
{
	// We always say that our certificate is valid, so we don't need CAs.
	public static bool ValidationCallback(System.Security.Cryptography.X509Certificates.X509Certificate certificate, int[] certificateErrors)
	{
		return true ;
	}

	public static void  Main(System.String[] args)
	{
		/* Check if we have the correct number of command line arguments */
		if (args.Length < 4)
		{
			System.Console.Error.WriteLine("Usage:   mono PGControl <host name> <login dn>" + " <password> <container> [ssl]");
			System.Console.Error.WriteLine("Example: mono PGControl Acme.com \"cn=admin,o=Acme\" secret" + " \"ou=Sales,o=Acme\"");
			System.Console.Error.WriteLine("\tfor test over a secure connection add SSL argument");
			System.Console.Error.WriteLine("Example: mono PGControl Acme.com \"cn=admin,o=Acme\" secret" + " \"ou=Sales,o=Acme\" ssl");
			System.Environment.Exit(1);
		}

		/* Parse the command line arguments  */
		System.String LdapHost = args[0];
		System.String loginDN = args[1];
		System.String password = args[2];
		System.String searchBase = args[3];
		System.Boolean ssl = false;

		if (args.Length == 5 && String.Equals(args[4], "ssl", StringComparison.OrdinalIgnoreCase))
			ssl = true;

		/*System.String LdapHost = "23.20.46.132";
		System.String loginDN = "cn=read-only-admin, dc=example,dc=com";
		System.String password = "password";
		System.String searchBase = "dc=example,dc=com";*/

		/*System.String LdapHost = @"192.168.50.133";
		System.String loginDN = @"test@rem.dev";
		System.String password = @"admin1!";
		System.String searchBase = @"dc=rem,dc=dev";*/

		int LdapPort = LdapConnection.DEFAULT_PORT;

		// If user asked for LDAPS, change the port
		if (ssl)
			LdapPort = LdapConnection.DEFAULT_SSL_PORT;

		int LdapVersion = LdapConnection.Ldap_V3;
		LdapConnection conn = new LdapConnection();

		try
		{
			// turn SSL on/off
			conn.SecureSocketLayer = ssl;
			// We don't require a valided SSL certificate to run the sample
			// If our certificated is not validated by a CA, we want to validate it ourselves.
			if (ssl)
				conn.UserDefinedServerCertValidationDelegate += new CertificateValidationCallback(ValidationCallback);

			conn.Connect(LdapHost, LdapPort);
			// bind to the server
			conn.Bind(LdapVersion, loginDN, password);
			System.Console.Out.WriteLine("Successfully logged in to server: " + LdapHost);

			/*
			 * Set default filter - Change this line if you need a different set
			 * of search restrictions. Read the "NDS and Ldap Integration Guide"
			 * for information on support by Novell eDirectory of this
			 * functionaliry.
			 */
			System.String MY_FILTER = "cn=*";

			/* 
			 * We are requesting that the givenname and cn fields for each 
			 * object be returned
			 */
			System.String[] attrs = new System.String[3];
			attrs[0] = "givenName";
			attrs[1] = "cn";
			attrs[2] = "gidNumber";

			// We will be sending two controls to the server 
			LdapSearchConstraints cons = conn.SearchConstraints;

			// hardcoded results page size
			int pageSize = 2;
			// initially, cookie must be set to an empty string
			System.String cookie = "";

			do
			{
				LdapControl[] requestControls = new LdapControl[1];
				requestControls[0] = new LdapPagedResultsControl(pageSize, cookie);
				cons.setControls(requestControls);
				conn.Constraints = cons;

				// Send the search request - Synchronous Search is being used here 
				//System.Console.Out.WriteLine("Calling Asynchronous Search...");
				LdapSearchResults res = conn.Search(searchBase, LdapConnection.SCOPE_SUB, MY_FILTER, attrs, false, (LdapSearchConstraints) null);

				// Loop through the results and print them out
				while (res.hasMore())
				{
				
					/* 
					 * Get next returned entry.  Note that we should expect a Ldap-
				     * Exception object as well, just in case something goes wrong
				     */
					LdapEntry nextEntry=null;
					try
					{
						nextEntry = res.next();
					}
					catch (LdapException e)
					{
						if (e is LdapReferralException)
							continue;
						else
						{
							System.Console.Out.WriteLine("Search stopped with exception " + e.ToString());
							break;
						}
					}

					/* Print out the returned Entries distinguished name.  */
					System.Console.Out.WriteLine();
					System.Console.Out.WriteLine(nextEntry.DN);

					/* Get the list of attributes for the current entry */
					LdapAttributeSet findAttrs = nextEntry.getAttributeSet();

					/* Convert attribute list to Enumeration */
					System.Collections.IEnumerator enumAttrs = findAttrs.GetEnumerator();
					System.Console.Out.WriteLine("Attributes: ");

					/* Loop through all attributes in the enumeration */
					while (enumAttrs.MoveNext())
					{
						LdapAttribute anAttr = (LdapAttribute) enumAttrs.Current;

						/* Print out the attribute name */
						System.String attrName = anAttr.Name;
//						if (attrName != "cn")
//							continue;
//						System.Console.Out.Write("\t{0}: ", attrName);
						System.Console.Out.Write("" + attrName);

						// Loop through all values for this attribute and print them
						System.Collections.IEnumerator enumVals = anAttr.StringValues;
						while (enumVals.MoveNext())
						{
							System.String aVal = (System.String) enumVals.Current;
							System.Console.Out.Write(" = {0}; ", aVal);
						}
						System.Console.Out.WriteLine("");
					}
				}

				// Server should send back a control irrespective of the 
				// status of the search request
				LdapControl[] controls = res.ResponseControls;
				if (controls == null)
				{
					System.Console.Out.WriteLine("No controls returned");
				}
				else
				{
					// Multiple controls could have been returned
					foreach(LdapControl control in controls)
					{
						/* Is this the LdapPagedResultsResponse control? */
						if (control is LdapPagedResultsResponse)
						{
							LdapPagedResultsResponse response = new LdapPagedResultsResponse(control.ID, control.Critical, control.getValue());

							cookie = response.Cookie;

							// Cookie is an opaque octet string. The chacters it contains might not be printable.
							byte[] hexCookie = System.Text.Encoding.ASCII.GetBytes(cookie);
							StringBuilder hex = new StringBuilder(hexCookie.Length);
							foreach (byte b in hexCookie)
								hex.AppendFormat("{0:x}", b);

							System.Console.Out.WriteLine("Cookie: {0}", hex.ToString());
							System.Console.Out.WriteLine("Size: {0}", response.Size);
						}
					}
				}
			// if cookie is empty, we are done.
			} while (!String.IsNullOrEmpty(cookie));

			/* We are done - disconnect */
			if (conn.Connected)
				conn.Disconnect();
		}
		catch (LdapException e)
		{
			System.Console.Out.WriteLine(e.ToString());
		}
		catch (System.IO.IOException e)
		{
			System.Console.Out.WriteLine("Error: " + e.ToString());
		}
		catch(Exception e)
		{
			System.Console.WriteLine("Error: " + e.Message);
		}
	}
}