# Dental Payment Calculation API

1. In [DentalPaymentCalculationApi.http](./DentalPaymentCalculationApi.http),
   set `@baseUrl` to `http://localhost:5085`, enter the college code and
   affiliation type, and enter the Base64-encoded Basic Auth value.

2. Send either request using the VS Code REST Client extension.

## Requests

### Detailed fee calculation and payment

```http
GET {{baseUrl}}/api/dental-payment-calculation?collegeCode={{collegeCode}}&affiliationTypeId={{affiliationTypeId}}
Authorization: Basic {{basicAuth}}
Accept: application/json
```

`collegeCode` is required and must not be greater than `D037`.
`affiliationTypeId` and `courseLevel` are optional. Append `&courseLevel=UG`
or `&courseLevel=PG` to filter by course level. A missing college code,
unsupported code, or invalid filter returns `400 Bad Request`.

### Summary

```http
GET {{baseUrl}}/api/dental-payment-calculation/summary?collegeCode={{collegeCode}}&affiliationTypeId={{affiliationTypeId}}&courseLevel=UG
Authorization: Basic {{basicAuth}}
Accept: application/json
```

## Example responses

Values below are illustrative.

### Detailed response (`200 OK`)

```json
{
  "collegeCode": "YOUR_CODE",
  "collegeName": "Example Dental College",
  "isGovernmentCollege": false,
  "results": [
    {
      "affiliationTypeId": 1,
      "affiliationCategory": "Continuation",
      "courseLevel": "UG",
      "matchedCourseCount": 1,
      "totalIntakeSeats": 100,
      "grandTotal": 125000.00,
      "payment": {
        "paymentId": 12,
        "transactionId": "TXN-EXAMPLE",
        "amountPaid": 125000.00,
        "hasReceipt": true
      },
      "feeLines": [
        {
          "feeTypeId": 3,
          "feeType": "Annual Fee",
          "totalAmount": 125000.00,
          "items": [
            {
              "id": 45,
              "courseCode": 101,
              "courseName": "Example Course",
              "courseLevel": "UG",
              "amountToBePaid": 1250.00,
              "academicIntake2026": 100,
              "calculationType": "Per Seat",
              "calculatedAmount": 125000.00
            }
          ]
        }
      ]
    }
  ]
}
```

### Summary response (`200 OK`)

```json
{
  "collegeCode": "YOUR_CODE",
  "collegeName": "Example Dental College",
  "isGovernmentCollege": false,
  "results": [
    {
      "affiliationTypeId": 1,
      "affiliationCategory": "Continuation",
      "courseLevel": "UG",
      "matchedCourseCount": 1,
      "totalIntakeSeats": 100,
      "grandTotal": 125000.00,
      "transactionId": "TXN-EXAMPLE",
      "amountPaid": 125000.00,
      "hasPayment": true,
      "hasReceipt": true
    }
  ]
}
```

The response intentionally omits the receipt's server-side file path.

## Status codes

| Status | Meaning |
|---|---|
| `200 OK` | Matching fee calculation returned |
| `400 Bad Request` | Missing college code or invalid filter |
| `401 Unauthorized` | Missing or invalid Basic Auth credentials |
| `404 Not Found` | No active fee rows match the requested filters |
| `503 Service Unavailable` | Dental API credentials are not configured in the running environment |
