using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.Utilities.Common
{
    public class Constants
    {
        public const string DefaultDateFormat = "{0:MM/dd/yyyy}";

        public const string DropDownDefaultItemLabel = "Select";

        public const string NotApplicableText = "N/A";

        public const int DefaultNumberOfRevisions = 1;
        public const int DefaultRevision = 1;

        public const int DefaultListPageSize = 10;
        public const int DefaultSmallListPageSize = 5;
        public const int DefaultListPage = 1;

        public const bool DefaultEntityInactive = false;
        public const bool DefaultEntityActive = true;

        public const string UserInterfaceBaseContainerId = "TestProject_container";

        public const string DefaultUrlRegX = @"^( )*http(s)?:\/\/[\w-]+([\.|\:][\w-]+)+((\/)(.)*)?$";     //@"^http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=#!*]*)?$";
        public const string DefaultEmailRegX = @"^( )*[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?\.)+[A-Za-z0-9](?:[A-Za-z0-9-]*[A-Za-z0-9])?( )*$";
        public const string DefaultPhoneRegX = @"^(\d+( )*)+$";

        public const string DefaultAppName = "TestProject SCIP";

        public const string OrganizationFilter = "filterOrgName";
        public const string MeetingOrganizationFilter = "filterOrganizationName";

        public const string AdministratorRole = "Administrator";
        public const string SupervisorRole = "Supervisor";
        public const string AgentRole = "Agent";

        private static List<KeyValuePair<string, string>> _inherentRoles;

        /// <summary>
        /// Statuses in which entity can be viewed
        /// </summary>
        public static int[] ViewableEntityStatuses
        {
            get { return new int[] {(int) EntityStatus.Active, (int) EntityStatus.Inactive}; }
        }

        /// <summary>
        /// Statuses in which entity can be edited.
        /// </summary>
        public static int[] EditableEntityStatuses
        {
            get { return new int[] { (int)EntityStatus.Active, (int)EntityStatus.Inactive }; }
        }

        /// <summary>
        /// Statuses in which entity can be deleted.
        /// </summary>
        public static int[] DeletableEntityStatuses
        {
            get { return new int[] { (int)EntityStatus.Active, (int)EntityStatus.Inactive }; }
        }

        /// <summary>
        /// Review statuses in which an entity can be edited.
        /// </summary>
        public static int[] EditableRevisionStatus
        {
            get { return new int[] { (int)RevisionStatus.Draft, (int)RevisionStatus.Rejected }; }
        }
        /// <summary>
        /// 
        /// </summary>
        public static List<KeyValuePair<string, string>> InherentRoles
        {
            get
            {
                if (_inherentRoles == null)
                {
                    _inherentRoles = new List<KeyValuePair<string, string>>();
                    _inherentRoles.Add(new KeyValuePair<string, string>(AdministratorRole.ToString(), AgentRole.ToString()));
                    _inherentRoles.Add(new KeyValuePair<string, string>(AdministratorRole.ToString(), SupervisorRole.ToString()));
                    _inherentRoles.Add(new KeyValuePair<string, string>(SupervisorRole.ToString(), AgentRole.ToString()));
                }
                return _inherentRoles;
            }
        }
        public const string BusinessAssemblyName = "TestProject.Business";
        public const string UiAssemblyName = "TestProject.Web";

        #region ViewBag | TempData Labels

        public const string UserTicketSessionId = "CurrentUserTicketSession";
        public const string ErrorBagLabel = "Error";
        public const string InfoBagLabel = "UserInfo";
        public const string CreateAbstractPostDataHolderLabel = "CreateAbstractModelData";

        #endregion

        public const string EncryptedParameterName = "psc";
        public const string BaseFileDirConfigName = "FilesBasePath";

        public const string AbstractBookFileTypes = ".jpg,.jpeg,.png,.ppt,.pptx,.pdf,.html,.doc,.docx";
        public const string AbstractAttachmentFileTypes = ".jpg,.jpeg,.png,.ppt,.pptx,.pdf,.html,.doc,.docx";
        public const int AbstractBookMaxFileSizeInMb = 250;
        public const int AbstractAttachmentMaxFileSizeInMb = 5;

        public const string AbstractImageFileTypes = ".jpg,.jpeg,.png";
        public const int AbstractImageMaxFileSizeInMb = 5;

        public const string EventProgramAttachmentFileTypes = ".jpg,.jpeg,.png,.ppt,.pptx,.pdf,.html,.doc,.docx";
        public const int EvnetProgramAttachmentMaxFileSizeInMb = 20;
    }
}
