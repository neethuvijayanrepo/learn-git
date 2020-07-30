using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TestProject.Utilities.Common
{
    public class Messages
    {
        public enum CommonMessages
        {
            [Display(Name = "Error occurred")]
            MsgErrorOccured,
            [Display(Name = "Request seems to be invalid")]
            MsgErrorInvalidRequest,
            [Display(Name = "Invalid action request. Please contact your Administrator")]
            MsgErrorConfirmWindowInvalid
        }
        public enum UserRoleMessages
        {
            [Display(Name = "Invalid Role Assignment ")]
            MsgErrorRoleInvalid,
            [Display(Name = "Role not found")]
            MsgErrorRoleAllocationDataNotFound,
            [Display(Name = "Role is not in updatable status")]
            MsgErrorRoleAllocationCantUpdate,
            [Display(Name = "Role updated successfully")]
            MsgInfoRoleAllocationUpdated,
            [Display(Name = "Role update failed")]
            MsgErrorRoleUpdateFailed,
            [Display(Name = "UserRole data not found")]
            MsgErrorUserRoleDataNotFound,
            [Display(Name = "Error while reading user role paged list")]
            MsgErrorUserRoleListRead,
            [Display(Name = "Don't have permission to change your own role")]
            MsgErrorCurrentUserRoleChange,
            [Display(Name = "This user is already having an active QC allocation. Role can be lifted only after the QC is completed.")]
            MsgErrorQCAllocatedForThisUser

        }
        public enum AuthenticationAuthorizationMessages
        {
            [Display(Name = "User authentication failed")]
            MsgErrorAuthenticationFailed,
            [Display(Name = "unauthorized access")]
            MsgErrorAuthorizationFailed,
            [Display(Name = "UnAuthorized User")]
            MsgErrorUnAuthorized,
            [Display(Name = "Invalid Credentials: Please try again")]
            MsgInvalidCredentials,
            [Display(Name = "You are not allowed to use PRISMOID")]
            MsgNotPrismoidUser,
            [Display(Name = "Our apologies, but we seem to be having a problem with some of your login information. If this message keeps recurring, please contact your administrator.")]
            MsgUnexpectedLoginProblems,
            [Display(Name = "This user account has been locked out.  You must contact your system administrator to re-establish this account.")]
            AccountLocked
        }

        public const string MsgErrorOccured = "Error occured";
        public const string MsgErrorInvalidRequest = "Request seems to be invalid";

        public const string MsgErrorAuthenticationFailed = "User authentication failed";

        public const string MSG_INVALID_CREDENTIALS = "Invalid Credentials: Please try again";
        public const string MSG_NOT_A_TestProject_USER = "You are not allowed to use TestProject";
        public const string MSG_UNEXPECTED_LOGIN_PROBLEMS = "Our apologies, but we seem to be having a problem with some of your login information. If this message keeps recurring, please contact your administrator.";
        public const string LOCKED_ACCOUNT = "This user account has been locked out.  You must contact your system administrator to re-establish this account.";

        public const string MsgErrorOrganizationDetailsInvalid = "Invalid organization";
        public const string MsgInfoOrganizationDetailsCreated = "Organization created successfully";
        public const string MsgErrorOrganizationNameExists = "Organization name already exists";
        public const string MsgErrorOrganizationAbbreviationExists = "Organization abbreviation already exists";
        public const string MsgErrorOrganizationCreate = "Error while creating organization";
        public const string MsgErrorOrganizationListRead = "Error while reading organization paged list";
        public const string MsgErrorOrganizationDataNotFound = "Organization not found";
        public const string MsgErrorOrganizationCantUpdate = "Organization is not in updatable status";
        public const string MsgErrorOrganizationUpdateFailed = "Organization update failed";
        public const string MsgInfoOrganizationDetailsUpdated = "Organization updated successfully";
        public const string MsgErrorOrganizationUpdatePartiallyFailed = "Organization update partially failed";
        public const string MsgErrorOrganizationCantInactive = "Organization can't be inactive, because its having published revisions. Please archive it and try again.";
        public const string MsgErrorOrganizationInactiveFailed = "Organization inactivation failed";
        public const string MsgInfoOrganizationInactivated = "Organization inactivated successfully";
        public const string MsgErrorOrganizationActiveFailed = "Organization activation failed";
        public const string MsgInfoOrganizationActivated = "Organization activated successfully";
        public const string MsgErrorOrganizationDeleteFailed = "Unable to delete, since this ORGANIZATION is linked to a MEETING which has only one associated organization  (There should atleast one associated Organization for a Meeting).";
        public const string MsgErrorMetOrgDeleteFailed = "Unable to delete. There should be atleast one associated organization for a meeting";
        public const string MsgInfoOrganizationDeleted = "Organization deleted successfully";

        public const string MsgErrorOrganizationLinkInvalid = "Organization link seems to be invalid";
        public const string MsgErrorOrganizationLinkCreateFailed = "Organization link creation failed";
        public const string MsgInfoOrganizationLinkCreateSuccess = "Organization link created successfully";
        public const string MsgErrorOrganizationLinkListRead = "Error while reading organization links";
        public const string MsgInfoOrganizationLinkListEmpty = "It seems like Organization is independent, no links found.";
        public const string MsgErrorOrganizationLinkDeletePartiallyFailed = "Organization link delete seems to be failed in between";
        public const string MsgInfoOrganizationLinkDeleteSuccess = "Organization link deleted successfully";
        public const string MsgErrorOrganizationLinkCreateInvalidLinkType = "Invalid link type provided";
        public const string MsgErrorOrganizationLinkCreateSameHierarchy = "Organization is already in same hierarchy";
        public const string MsgErrorOrganizationLinkCreateJointVentureNotPossible = "Organization is already in parent child relation and hence joint venture is not possible";
        public const string MsgErrorOrganizationLinkCreateLinkHasParent = "Linking organization is already having a parent";
        public const string MsgErrorOrganizationLinkCreateOrganizationHasParent = "Parent already exists for current organization";
        public const string MsgErrorOrganizationLinkCreateLinkAlreadyExists = "Link between the organizations already exists";
        public const string MsgErrorOrganizationLinkCreateSelfLink = "Self link is not possible";

        public const string MsgErrorConfirmWindow = "Invalid action request. Please contact your Administrator";
        public const string MsgErrorConfirmWindowInvalid = "Invalid action request";

        public const string MSG_CONTACT_DATANOTFOUND = "Organization contact details not found";
        public const string MSGERROR_ORGCONTACT_CANTUPDATE = "Organization contact details is not in updatable status";
        public const string MSG_INVALID_CONTACT_DETAILS = "Invalid organization contact details";
        public const string MSG_ORGANIZATIONCONTACT_INFO_CREATED = "Organization Contact created successfully";
        public const string MSG_ERROR_ORGANIZATIONCONTACT_CREATE = "Organization Contact creation failed";
        public const string MSG_ORGANIZATIONCONTACT_INFO_UPDATED = "Contact updated successfully";
        public const string MSG_ORGANIZATIONCONTACT_INFO_UPDATE_FAILED = "Contact update  failed";

        public const string MSG_COUNTRY_DATANOTFOUND = "Country details not found";

        public const string MSG_ORGANIZATIONURL_DATANOTFOUND = "Organization url details not found";
        public const string MSGERROR_ORGURL_CANTUPDATE = "Organization contact details is not in updatable status";
        public const string MSG_INVALID_URL_DETAILS = "Invalid organization url details";
        public const string MSG_ORGANIZATIONURL_INFO_CREATED = "organization URL created successfully";
        public const string MSG_ORGANIZATIONURL_CREATION_FAILED = "Failed URL creation";
        public const string MSG_ORGANIZATIONURL_INFO_UPDATED = "organization URL updated successfully";
        public const string MSG_ORGANIZATIONURL_UPDATION_FAILED = "Failed URL creation";
        public const string MSG_ORGANIZATIONURL_INFO_DELETED = "organization URL deleted successfully";
        public const string MSG_ORGANIZATIONURL_DELETION_FAILED = "Failed URL deletion";
        public const string MsgError_OrgUrl_ListRead = "Error while reading organization url paged list";
        public const string MsgErrorOrganizationUrlNameExists = "Organizationurl name already exists";

        public const string MsgErrorMettingOrganization = "No Organization is added with this meeting.";
        public const string MsgErrorMeetingNameExists = "Meeting name already exists";
        public const string MsgErrorMeetingNameAlredyExists = "Meeting name already exists in the organization  ";
        public const string MsgErrorMeetingAbbreviationExists = "Meeting abbreviation already exists";
        public const string MsgInfoMeetingCreated = "Meeting created successfully";
        public const string MsgErrorMeetingCreate = "Error while creating meeting";
        public const string MsgErrorMeetingDetailsInvalid = "Invalid meeting";
        public const string MsgErrorMeetingDataNotFound = "Meeting not found";
        public const string MsgInfoMeetingDeleteSuccess = "Meeting  deleted successfully";
        public const string MsgErrorMeetingDeleteFailed = "Meeting deletion failed";
        public const string MsgErrorMeetingCantUpdate = "Meeting is not in updatable status";
        public const string MsgErrorMeetingUpdateFailed = "Meeting update failed";
        public const string MsgInfoMeetingDetailsUpdated = "Meeting updated successfully";
        public const string MsgErrorMeetingUpdatePartiallyFailed = "Meeting update partially failed";
        public const string MsgErrorMeetingInactiveFailed = "Meeting inactivation failed";
        public const string MsgInfoMeetingInactivated = "Meeting inactivated successfully";
        public const string MsgErrorMeetingActiveFailed = "Meeting activation failed";
        public const string MsgInfoMeetingActivated = "Meeting activated successfully";
        public const string MsgErrorMeetingCantInactive = "Meeting can't be inactive, because its having published revisions. Please archive it and try again.";
        public const string MsgErrorMeetingSameOrg = "Same Organization is Linked multiple times with the meeting";

        public const string MsgErrorInvalid = "Cannot add Organization with meeting";
        public const string MsgInfoMeetingOrganizationCreateSuccess = "Meeting Organization  created successfully";
        public const string MsgErrorMeetingOrganizationCreateFailed = "Meeting Organization creation failed";
        public const string MsgInfoMeetingOrganizationDeleteSuccess = "Meeting Organization  deleted successfully";
        public const string MsgErrorMeetingOrganizationDeleteFailed = "Meeting Organization deletion failed";
        public const string MsgErrorMeetingOrganizationListRead = "Error while reading MeetingOrganization";
        

        public const string MsgErrorEventDetailsInvalid = "Invalid event";
        public const string MsgInfoEventCreated = "Event created successfully";
        public const string MsgErrorEventCreate = "Error while creating event";
        public const string MsgErrorEventExists = "Event with same name & date already exists under the selected Meeting";
        public const string MsgErrorEventDataNotFound = "Event not found";
        public const string MsgErrorEventListRead = "Error while reading event paged list";
        public const string MsgInfoEventDeleteSuccess = "Event  deleted successfully";
        public const string MsgErrorEventActiveFailed = "Event activation failed";
        public const string MsgInfoEventActivated = "Event activated successfully";
        public const string MsgInfoEventInactivated = "Event inactivated successfully";
        public const string MsgErrorEventDeleteFailed = "Event deletion failed";
        public const string MsgErrorEventCantUpdate = "Event is not in updatable status";
        public const string MsgErrorEventUpdateFailed = "Event update failed";
        public const string MsgInfoEventDetailsUpdated = "Event updated successfully";
        public const string MsgErrorEventUpdatePartiallyFailed = "Event update partially failed";
        public const string MsgErrorEventDetails = "Error while reading event details";
        public const string MsgInfoEventIngestionCompleted = "The event abstract ingestion has completed.";


        public const string MsgErrorEventLocationDetailsInvalid = "Invalid EventLocation";
        public const string MsgErrorEventLocationDataNotFound = "EventLocation not found";
        public const string MSG_EventLocation_Created = "EventLocation created successfully";
        public const string MSG_Error_EventLocation_Create = "EventLocation creation failed";
        public const string Msgerror_EventLocation_Cantupdate = "EventLocation is not in updatable status";
        public const string MsgInfoEventLocationUpdated = "EventLocation updated successfully";
        public const string MsgErrorEventLocationUpdationfailed = "EventLocation update failed";
        public const string MsgErrorEventLocationDeletionFailed = "EventLocation deletion failed";
        public const string MsgInfoEventLocationDeleted = "EventLocation deleted successfully";

        public const string MsgErrorEventAbstractBookProcessing = "Error while processing abstract book";
        public const string MsgErrorEventAbstractBookAttachement = "Event abstract book attachment failed";
        public const string MsgInfoEventAbstractBookAttachement = "Abstract book attached successfully in event";
        public const string MsgErrorAbstractbookNotFound = "Abstractbook is not found in input";
        public const string MsgErrorAbstractbookUpdateFailed = "Abstractbook update failed";
        public const string MsgInfoAbstractbookUpdated = "Abstractbook updated successfully";
        public const string MsgErrorAbstractBookListRead = "Error while reading abstract book list";
        public const string MsgErrorAbstractBookDataNotFound = "Abstract Book not found";
        public const string MsgErrorAbstractBookDeleteFailed = "Error while deleting abstract book";
        public const string MsgInfoAbstractBookDeleted = "Abstract book deleted successfully";
        public const string MsgErrorAbstractBookInvalidDetails = "Invalid abstract book details provided";
        public const string MsgErrorAbstractBookCreate = "Abstract Book creation failed";
        public const string MsgErrorAbstractBookCreatePartial = "Abstract Book creation partialy failed";
        public const string MsgInfoAbstractBookCreated = "Abstract Book(s) created successfully";

        public const string MsgErrorAbstractListRead = "Error while reading Abstract paged list";
        public const string MsgErrorAbstractDeleteFailed = "Abstract deletion failed";
        public const string MsgInfoAbstractDeleteSuccess = "Abstract  deleted successfully";
        public const string MsgErrorAbstractDataNotFound = "Abstract not found";
        public const string MsgInfoAbstractInactivated = "Abstract inactivated successfully";
        public const string MsgErrorEventAbstractUrlNameExists = "Abstract url name already exists";
        public const string MsgInfoEventAbstractUrlCreated = "Abstract url created successfully";
        public const string MsgInfoEventAbstractUrlCreationFailed = "Abstract url creation failed";
        public const string MsgErrorEventAbstractUrlListNotFound = "No abstract url found";
        public const string MsgErrorEventAbstractUrlDataNotFound = "Abstract url not found";
        public const string MsgErrorEventAbstractUrlDeletionFailed = "Abstract url deletion failed";
        public const string MsgInfoEventAbstractUrlDeleted = "Abstract url deleted successfully";
        public const string MsgInfoEventAbstractUrlCollected = "Abstract url collected successfully";
        public const string MsgInfoDataNotFound = "Abstract url details not found";
        public const string MsgErrorEvtabsturlCantupdate = "Abstract url details is not in updatable status";
        public const string MsgInfoEventAbstractUrlUpdated = "Abstract url updated successfully";
        public const string MsgInfoEventAbstractUrlUpdationFailed = "Abstract url updation failed";
        public const string MsgErrorAbstractInactionFailed = "Abstract inactivation failed";
        public const string MsgErrorAbstractActiveFailed = "Abstract activation failed";
        public const string MsgInfoAbstractActivated = "Abstract activated successfully";
        public const string MsgErrorAbstractAddEventSelection = "Invalid abstract add request";
        public const string MsgErrorAbstractInvalid = "Invalid Abstract";
        public const string MsgErrorAbstractCantUpdate = "Abstract is not in updatable status";
        public const string MsgErrorAbstractUpdateFailed = "Abstract update failed";
        public const string MsgInfoAbstractUpdated = "Abstract updated successfully";
        public const string MsgErrorAbstractPartiallyFailed = "Abstract update partially failed";
        public const string MsgErrorAbstractNumExists = "Abstract Number already exists";
        public const string MsgInfoAbstractAdded = "Abstract added successfully";
        public const string MsgErrorAbstractAddFailed = "Error while adding abstract";
        public const string MsgErrorAbstractTitleExists = "Abstract title already exists";
        public const string MsgErrorAbstractInvalidDetails = "Invalid abstract details";
        public const string MsgErrorAbstractAddNotingToIngest = "It seems like there is nothing to ingest in the event. If you are having some abstracts to ingest please try after updating the number of abstracts available in event.";
        public const string MsgErrorAbstractAddIngestionCompleted = "It seems like event abstract ingestion is completed. If you are having some abstracts to ingest please try after updating the number of abstracts available in event.";


        public const string MsgErrorAttachmentFileRequired = "Attachment file should be included as stream";
        public const string MsgErrorAttachmentFilePathRequired = "Attachment existing file path should be included";
        public const string MsgErrorAttachmentPermanentOfTempType = "Permanent attachments of temp type can not be created";
        public const string MsgErrorAttachmentFilePhysicalSaveError = "Error while saving physical attachment file";
        public const string MsgInfoAttachmentCreated = "Attachment created successfully";
        public const string MsgInfoAttachmentTempCreated = "Temp file created successfully";
        public const string MsgErrorAttachmentCreate = "Error while creating attachment";
        public const string MsgErrorAttachmentUpdate = "Error while updating attachment";
        public const string MsgErrorAttachmentRead = "Attachment not found";
        public const string MsgErrorAttachmentReadFileNotFound = "Attachment file not found";
        public const string MsgInfoAttachmentRead = "Attachment collected successfully";
        public const string MsgErrorAttachment = "Invalid attachment add request";
        public const string MsgErrorAttachmentNotFound = "Attachment is not found in input";
        public const string MsgErrorAttachmentUpdateFailed = "Attachment update failed";
        public const string MsgInfoAttachmentDetailsUpdated = "Attachment updated successfully";
        public const string MsgErrorAttachmentDeleteFailed = "Attachment Deletion failed";
        public const string MsgInfoAttachmentDetailsDeleted = "Attachment Deleted successfully";
        public const string MsgErrorAttachmentUpdatePartiallyFailed = "Attachment update partially failed";

        public const string MsgErrorAffiliationDataNotFound = "Abstract Author Affiliation Details not found";
        public const string MsgErrorAbstractAuthorDeleteFailed = "Abstract Author deletion failed";
        public const string MsgErrorAbstractAuthorDeleteSingleAuthor = "There is only one author assigned for the abstract and hence it cannot be deleted";
        public const string MsgErrorAbstractAuthorDataNotFound = "Abstract Author not found";
        public const string MsgErrorAbstractAuthorListRead = "Error while reading AbstractAuthors";
        public const string MsgInfoAbstractAuthorDeleteSuccess = "Abstract Author  deleted successfully";
        public const string MsgErrorAbstractAuthorInvalid = "Invalid Author Details";
        public const string MsgInfoAbstractAuthorAdded = "Abstract Author added successfully";
        public const string MsgErrorAbstractAuthorAddFailed = "Error while adding abstract author";
        public const string MsgErrorAbstractAuthorCantupdate = "Abstract Author details is not in updatable status";
        public const string MsgInfoAbstractAuthorUpdated = "Abstract Author updated successfully";
        public const string MsgInfoAbstractAuthorUpdationfailed = "Abstract Author update failed";

        public const string MsgErrorCreatingFile = "Error while creating attachment File";
        public const string MsgErrorCreatingFileInputNull = "File details are null";
        public const string MsgErrorCreatingFileNameNotProvided = "File name not provided";
        public const string MsgErrorCreatingFileDataNotProvided = "File data bytes are not provided";
        public const string MsgErrorFileTypeDirectory = "Error while initializing attachment type directory";

        public const string MsgErrorMovingFile = "Error while moving attachment File";
        public const string MsgErrorMovingFilePathNull = "Existing file path is not provided";

        public const string MsgErrorAbstractImageListRead = "Error while reading abstract image list";
        public const string MsgErrorAbstractImagesDataNotFound = "Abstract images not found";
        public const string MsgErrorAbstractImageNotFound = "Abstract image not found";
        public const string MsgInfoAbstractImageDeleted = "Abstract image deleted successfully";
        public const string MsgErrorAbstractImageDeleteFailed = "Error while deleting abstract image";
        public const string MsgErrorAbstractImageInvalidDetails = "Invalid abstract image details provided";
        public const string MsgErrorAbstractImageCreate = "Abstract image creation failed";
        public const string MsgErrorAbstractImageCreatePartial = "Abstract image creation partialy failed";
        public const string MsgInfoAbstractImageCreated = "Abstract image(s) created successfully";
        

    }
}
