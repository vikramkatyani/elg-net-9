# Risk Assessment File Upload - Error Handling Improvements

## Overview
Enhanced the Risk Assessment file upload feature to provide better error handling and support for both Excel (.xlsx) and CSV file formats. Users now receive specific, actionable error messages instead of generic "File contains corrupted data" errors.

## Changes Made

### 1. **RiskAssessmentExcelParser.cs** - Enhanced Parser
**Location**: `LMS_admin/Helper/RiskAssessmentExcelParser.cs`

**Key Improvements**:
- **Excel → CSV Fallback**: Parser attempts to read files as Excel first, then falls back to CSV format if Excel parsing fails
- **Column Validation**: Validates that required columns exist before processing (Section, Question, Instructions)
- **Detailed Error Messages**: Throws `InvalidOperationException` with specific column names when required headers are missing
  - Example: `"Missing required columns: Section, Question, Instructions"`
- **Graceful File Format Handling**: Supports both .xlsx and .csv files transparently

**Methods Added/Enhanced**:
- `ParseExcel()`: Parses Excel files using ClosedXML library
- `ParseCsv()`: Parses CSV files using CsvHelper library with fallback support
- `ValidateRequiredColumns()`: Checks for required column headers and provides detailed error messages
- `ProcessRows()`: Processes data rows from Excel
- `ProcessCsvRow()`: Processes individual CSV rows
- `ParseIssueIndices()`: Extracts comma-separated issue option indices

### 2. **RiskAssessmentController.cs** - Improved Error Handling
**Location**: `LMS_admin/Controllers/RiskAssessmentController.cs` (UploadDraft endpoint)

**Key Improvements**:
- **Differentiated Error Responses**:
  - `InvalidOperationException` (validation errors) → Returns `success=0` with specific error message
  - Other exceptions (file format/corruption) → Returns `success=-1` with helpful file format hint
  
- **User-Friendly Messages**:
  - Validation Error: "Missing required columns: Section, Question, Instructions"
  - Format Error: "Failed to parse file. Ensure it is a valid Excel (.xlsx) or CSV file with required columns: Section, Question, Instructions, Option1-Option6, Issue."
  - Empty File: "No questions found in the file. Ensure the file has correct headers and at least one question row."

- **Error Handling Flow**:
  ```
  File Upload → Check if null/empty → Copy to MemoryStream
  → Parser.Parse(stream)
    ├── If InvalidOperationException → Return validation error (success=0)
    ├── If other Exception → Return format error (success=-1)
    └── If success → Cache draft, return draftKey + parsed data
  ```

### 3. **LMS_admin.csproj** - Added CsvHelper NuGet Package
**Location**: `LMS_admin/LMS_admin.csproj`

**Added Dependency**:
```xml
<PackageReference Include="CsvHelper" Version="32.0.0" />
```

This enables the parser to handle CSV files with proper header mapping and value extraction.

### 4. **RiskAssessment_Template.csv** - Download Template
**Location**: `LMS_admin/wwwroot/templates/RiskAssessment_Template.csv`

**Template Structure**:
```
Section,Question,Instructions,Option1,Option2,Option3,Option4,Option5,Option6,Issue
General,What is your understanding of risk?,Select the best option,Excellent,Good,Fair,Poor,,1,2
General,Have you completed the safety training?,Review your training status,Yes,No,,,,,
```

**Column Descriptions**:
- **Section**: RA section grouping (e.g., "General")
- **Question**: Question text that users will answer
- **Instructions**: Guidance text shown with the question
- **Option1-Option6**: Answer choices (up to 6, leave blank if fewer needed)
- **Issue**: Comma-separated indices of options considered "issues" (e.g., "1,2" marks options 1 and 2 as problematic)

## Error Response Format

### Validation Error (Missing Columns)
```json
{
  "success": 0,
  "message": "Missing required columns: Section, Question, Instructions"
}
```
**Status**: 400 (user error - fixable by correcting file)

### File Format Error
```json
{
  "success": -1,
  "message": "Failed to parse file. Ensure it is a valid Excel (.xlsx) or CSV file with required columns: Section, Question, Instructions, Option1-Option6, Issue."
}
```
**Status**: 400 (user error - wrong file format)

### Success Response
```json
{
  "success": 1,
  "draftKey": "abc123def456",
  "data": {
    "Sections": [
      {
        "Name": "General",
        "Questions": [
          {
            "Question": "What is your understanding of risk?",
            "Instructions": "Select the best option",
            "Options": [
              {"Text": "Excellent", "Issue": true, "Order": 10},
              {"Text": "Good", "Issue": true, "Order": 20},
              ...
            ]
          }
        ]
      }
    ]
  }
}
```

## Client-Side JavaScript Handler

**Location**: `LMS_admin/wwwroot/Scripts/Admin/CourseManagement/ManageModule.js`

**createRAHandler IIFE**:
- Listens for file selection and updates label with filename
- Validates title and file before upload
- Calls `/RiskAssessment/UploadDraft` endpoint
- On validation error (success=0), displays error message and allows user to correct file
- On format error (success=-1), displays helpful message about file format
- On success, caches draft key and calls `/RiskAssessment/PublishDraft` to persist to database
- Refreshes module table on successful publish

## Testing Checklist

- [ ] Upload valid Excel file with correct columns → Should parse and create draft
- [ ] Upload valid CSV file with correct columns → Should parse and create draft
- [ ] Upload file with missing "Section" column → Should return "Missing required columns: Section..."
- [ ] Upload file with missing "Question" column → Should return "Missing required columns: Question..."
- [ ] Upload file with wrong extension (.doc, .txt) → Should return "Failed to parse file..."
- [ ] Upload empty Excel/CSV file → Should return "No questions found in the file..."
- [ ] Upload file with blank question rows → Should skip blank rows and only process questions with text
- [ ] Issue indices parsing: "1,2" should mark options 1 and 2 as issues ✓
- [ ] Multiple sections in same file → Should create separate sections correctly

## Deployment Notes

1. **NuGet Package**: CsvHelper (v32.0.0) must be restored during build
2. **Template File**: RiskAssessment_Template.csv must be accessible in wwwroot/templates/
3. **Database**: No schema changes required; uses existing tbCourse.blnCourseRA column
4. **Backward Compatibility**: Existing Excel-based upload continues to work

## Benefits

✅ **Better User Experience**:
- Clear, specific error messages instead of generic "corrupted data"
- Users know exactly what columns are missing or what format is required

✅ **Flexible File Format**:
- Support for both Excel and CSV files
- CSV fallback ensures compatibility if Excel parsing fails

✅ **Robust Error Handling**:
- Differentiates between validation errors (user-fixable) and format errors
- Prevents exposing internal exception details to end users

✅ **Improved Debugging**:
- Detailed logging in controller tracks validation vs. format errors
- Developer-friendly exception messages in parser

## Technical Stack

- **Parser Library**: ClosedXML (Excel) + CsvHelper (CSV)
- **Framework**: ASP.NET Core 9.0 MVC
- **Storage**: Session-based ConcurrentDictionary for draft cache
- **HTTP**: JSON response format for AJAX client calls
