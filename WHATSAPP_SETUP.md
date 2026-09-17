# WhatsApp Integration Setup Guide

## Current Status

The payment calculation flow is now configured to send payment details and PDF receipts via WhatsApp to the Principal's mobile number. The system will:

1. ✅ Generate a PDF receipt with calculation details
2. ✅ Fetch the Principal's mobile number from `AFF_InstitutionsDetails` table (CollegeCode match)
3. ✅ Send the receipt PDF via WhatsApp
4. ✅ Send the payment screenshot via WhatsApp
5. ✅ Log errors for debugging

## Required Setup

### 1. WhatsApp Phone Number ID

The most critical piece is obtaining your **WhatsApp Business Account Phone Number ID** from the WhatsApp Business API provider (in this case, aistudiocraft.in).

**To get this value:**
- Log in to your WhatsApp Business account on the aistudiocraft.in platform
- Navigate to settings or account information
- Look for "Phone Number ID" or "Phone Number Instance ID"
- This is a numeric value (e.g., `1234567890123`)

### 2. Update appsettings.json

Edit `/c:/Medical/Medical_Affiliation/appsettings.json` and update the `WhatsAppSettings` section:

```json
{
  "WhatsAppSettings": {
    "ApiBaseUrl": "https://apps.aistudiocraft.in/api/v1/whatsapp/send/file",
    "ApiToken": "23304|QpZtSMudpPTMxNX3FYxeXjZ1tuwGuGg1YYjCkDxSc2aa5807",
    "PhoneNumberId": "YOUR_PHONE_NUMBER_ID_HERE"
  }
}
```

Replace `YOUR_PHONE_NUMBER_ID_HERE` with your actual WhatsApp phone number ID.

### 3. Verify Principal Mobile Number in Database

The system fetches the principal's phone number from the `AFF_InstitutionsDetails` table. Ensure:

```sql
SELECT TOP (10) 
  CollegeCode, 
  PrincipalMobileNumber, 
  NameOfInstitution
FROM [Admission_Affiliation].[dbo].[AFF_InstitutionsDetails]
WHERE PrincipalMobileNumber IS NOT NULL
```

- The `PrincipalMobileNumber` field is populated for your colleges
- Phone numbers are stored as 10-digit Indian mobile numbers (or with +91 prefix)
- Examples: `9876543210` or `+919876543210`

## How It Works

When a user clicks "Save Payment Details" on the Payment Calculation page:

1. **Calculation PDF is generated** with:
   - Matched courses
   - Fee breakdown
   - Grand total
   - Transaction ID
   - Payment amount & date

2. **Principal contact is looked up** from database using CollegeCode:
   ```sql
   SELECT TOP (1) PrincipalMobileNumber, NameOfInstitution
   FROM [Admission_Affiliation].[dbo].[AFF_InstitutionsDetails]
   WHERE CollegeCode = @CollegeCode
   ```

3. **WhatsApp messages are sent:**
   - First message: Receipt PDF with caption "Payment receipt for [CollegeCode] — Transaction ID: [ID]"
   - Second message: Payment screenshot (if uploaded) with caption "Payment screenshot"

4. **Status is saved:**
   - Messages are tracked in the `PaymentReceipts` table
   - Status shows "Sent" or "Failed: [error message]"

## Debugging

If WhatsApp messages are not sending, check the browser console and server logs for:

```
[WhatsApp] Principal lookup for ABC123: Mobile=9876543210, Institution=ABC College
[WhatsApp] Sending document to 919876543210: PaymentReceipt_ABC123.pdf
[WhatsApp] Exception sending to 919876543210: [Error details]
```

Common issues:
- **"Principal mobile number not found"** → No matching college code in `AFF_InstitutionsDetails`
- **"PhoneNumberId is empty"** → `appsettings.json` not updated with actual Phone Number ID
- **"WhatsApp API token is not configured"** → ApiToken is missing or empty
- **"Invalid or missing phone number"** → Phone number in database is malformed

## Testing

1. Navigate to: `https://localhost:44363/PaymentCalculation/Index?level=PG`
2. Review the calculated payment details
3. Enter transaction ID, payment amount, payment date
4. Upload a payment screenshot (optional)
5. Click "Save Payment Details"
6. Check for WhatsApp messages sent to the Principal's number

## Flow Diagram

```
User clicks "Save Payment Details"
  ↓
PaymentDocumentController.SavePaymentDocument()
  ↓
Validate form data & save to PaymentAffiliationDocuments table
  ↓
SendPaymentDetailsToWhatsAppAsync()
  ├─ Look up PrincipalMobileNumber from AFF_InstitutionsDetails
  ├─ Generate receipt PDF using calculation snapshot
  ├─ Send receipt PDF via WhatsApp API
  ├─ Send payment screenshot via WhatsApp API
  └─ Save WhatsApp send status to PaymentReceipts table
  ↓
Return success/failure to browser
```

## Database Tables

### PaymentAffiliationDocuments
- Stores payment details and screenshot file path
- Linked to: CollegeCode, FacultyCode, CourseLevel, AffiliationTypeId

### PaymentReceipts
- Tracks WhatsApp send status for each payment
- Stores receipt PDF path and WhatsApp message IDs
- Contains `PublicAccessToken` for download links

### AFF_InstitutionsDetails (Admission_Affiliation database)
- Source of truth for principal contact information
- Key field: `PrincipalMobileNumber`

## Support

If WhatsApp messages still don't send after setup:
1. Verify `PhoneNumberId` in `appsettings.json`
2. Check `PrincipalMobileNumber` is populated in the database
3. Review browser console for 400/500 error responses
4. Check server logs for `[WhatsApp]` debug messages
