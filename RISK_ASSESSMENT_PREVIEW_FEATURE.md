# Risk Assessment Modal - Preview & Publish/Discard Feature

## Issues Fixed

### 1. "undefined" on Add Button
**Problem**: The Add button was displaying "undefined" text instead of the proper label.

**Root Cause**: The original button text was hardcoded but not properly displaying due to CSS or element state issues.

**Solution**: Redesigned the modal with separate Upload and Preview phases, replacing the single "Add" button with:
- **Upload Phase**: "Upload" button to validate and upload the file
- **Preview Phase**: "Publish" and "Discard" buttons for draft management

### 2. Missing Preview Feature
**Problem**: Users had no way to review the questions before publishing the Risk Assessment.

**Solution**: Added a Preview section that displays:
- Section grouping with folder icon
- Question text with numbering (Q1, Q2, etc.)
- Instructions/guidance text in italics
- All answer options in a list format
- **Issue badges** (red) marking options flagged as problematic
- Scrollable container for large datasets (max-height: 400px)

### 3. Missing Publish/Discard Buttons
**Problem**: The workflow was automatic - upload immediately triggered publish with no user review or option to discard.

**Solution**: Implemented a two-phase modal workflow:
- **Phase 1 (Upload)**: User uploads file and can see preview
- **Phase 2 (Review)**: User can Publish (persist to DB) or Discard (delete draft)

## UI Changes

### Modal Structure Changes

#### Previous State
```
Upload Content Section
├── Title input
├── Description input
├── File picker
└── Footer: [Close] [Add]
```

#### New Structure
```
Upload Content Section (Phase 1)
├── Title input
├── Description input
├── File picker
├── Template download link
└── Footer: [Close] [Upload]

Preview Risk Assessment Section (Phase 2)
├── Scrollable preview area
│   └── Section grouping with questions and options
└── Footer: [Discard] [Publish]
```

### Modal Size
- **Previous**: `modal-lg` (90% width, max 500px)
- **New**: `modal-xl` (97% width, max 1140px) + `modal-dialog-scrollable` for large content

## JavaScript Handler Enhancements

### Handler Flow

```
showCreateRAPopup()
  ↓
resetModal()
  ├─ Clear all inputs
  ├─ Hide preview section
  ├─ Show upload section
  └─ Show upload footer
  ↓
User clicks Upload
  ↓
Validate Title + File
  ├─ Check file exists
  ├─ Check file size (≤5MB)
  └─ Check title not empty
  ↓
UploadDraft AJAX Call
  ├─ Parse Excel/CSV file
  ├─ Cache draft with key
  └─ Return parsed questions
  ↓
renderPreview(parsedData)
  ├─ Generate HTML preview
  ├─ Show sections with questions
  ├─ Highlight issue options
  └─ Display in preview area
  ↓
showPreviewMode()
  ├─ Hide upload section
  ├─ Show preview section
  ├─ Swap footers
  └─ Enable publish/discard buttons
  ↓
User clicks Publish OR Discard
  │
  ├─→ Publish
  │   ├─ PublishDraft AJAX call
  │   ├─ Create RA course in DB
  │   ├─ Map as submodule
  │   └─ Close modal + refresh table
  │
  └─→ Discard
      ├─ DiscardDraft AJAX call
      ├─ Remove draft from cache
      └─ Reset modal (return to upload phase)
```

### Key Methods

**resetModal()**
- Clears all form fields
- Resets draft key
- Shows upload section, hides preview
- Clears any error/success messages

**renderPreview(data)**
- Builds HTML representation of Risk Assessment structure
- Shows sections as grouped containers
- Lists questions with instructions
- Highlights issue options with red badges
- Handles empty/null data gracefully

**showPreviewMode()**
- Hides upload section and footer
- Shows preview section and footer
- Enables user to review before committing

## API Endpoints Used

### 1. RiskAssessment/UploadDraft
- **Method**: POST
- **Accepts**: IFormFile (Excel/CSV)
- **Returns**: 
  ```json
  {
    "success": 1,
    "draftKey": "abc123",
    "data": { /* parsed questions */ }
  }
  ```
- **Errors**: 
  - `success=0`: Validation error (missing columns)
  - `success=-1`: File format error

### 2. RiskAssessment/PublishDraft
- **Method**: POST
- **Parameters**: 
  - `draftKey`: Draft identifier
  - `parentCourseId`: Course ID
  - `Title`: RA title
  - `Description`: RA description
- **Returns**: 
  ```json
  { "success": 1 }  // on success
  { "success": 0, "message": "error text" }  // on error
  ```

### 3. RiskAssessment/DiscardDraft
- **Method**: POST
- **Parameters**: `draftKey`
- **Returns**: 
  ```json
  { "success": 1 }  // always succeeds if key exists
  ```

## Preview Rendering Examples

### Input Data Structure
```json
{
  "Sections": [
    {
      "Name": "General",
      "Questions": [
        {
          "Question": "What is your understanding of risk?",
          "Instructions": "Select the best option",
          "Options": [
            { "Text": "Excellent", "Issue": true, "Order": 10 },
            { "Text": "Good", "Issue": true, "Order": 20 },
            { "Text": "Fair", "Issue": false, "Order": 30 }
          ]
        }
      ]
    }
  ]
}
```

### Rendered HTML Output
```
📁 General
  Q1: What is your understanding of risk?
  ℹ️ Select the best option
  ○ Excellent [Issue Badge]
  ○ Good [Issue Badge]
  ○ Fair
```

## Alerts and Messages

### Validation Errors (Upload Phase)
- "Please enter a title."
- "Please select a file (.xlsx or .csv)."
- "Selected file exceeds 5 MB limit."

### File Parse Errors
- "Missing required columns: Section, Question, Instructions"
- "Failed to parse file. Ensure it is a valid Excel (.xlsx) or CSV file..."
- "Failed to upload file. Please try again."

### Preview Phase
- "No questions found in the file." (displayed in preview)
- On Discard: "Draft discarded. You can upload another file."
- On Publish: "Risk Assessment published successfully!"

## Testing Checklist

- [ ] Modal opens with upload section visible
- [ ] File picker accepts .xlsx and .csv files
- [ ] Template download link works
- [ ] Upload button validates title field
- [ ] Upload button validates file selection
- [ ] Upload button validates file size (≤5MB)
- [ ] Successful upload shows preview section
- [ ] Preview displays sections correctly
- [ ] Preview displays all questions
- [ ] Preview shows options with proper formatting
- [ ] Issue options highlighted with red badge
- [ ] Discard button removes draft and resets modal
- [ ] Publish button creates RA course and closes modal
- [ ] Table refreshes after successful publish
- [ ] Error messages display correctly
- [ ] Modal closes properly after publish
- [ ] Closing modal without publish loses draft

## Technical Details

### State Variables
- `selectedCourseId`: Current course being configured
- `draftKey`: Identifier for uploaded file cache
- `$modal`, `$title`, `$alert`, etc.: jQuery element references

### CSS Classes Used
- `.collapse`: Bootstrap class to hide/show sections
- `.bg-light`, `.rounded`: Preview styling
- `.text-danger`: Issue option highlighting
- `.badge.bg-danger`: Issue badge styling

### Dependencies
- jQuery for DOM manipulation
- Bootstrap modals and alerts
- UTILS library for button state management
- CsvHelper/ClosedXML libraries (backend) for file parsing

## Migration from Previous Implementation

| Aspect | Before | After |
|--------|--------|-------|
| Button Label | "Add" (undefined) | "Upload" then "Publish"/"Discard" |
| Preview | None | Full question/option preview |
| User Review | Auto-publish | Review before publish option |
| Modal Size | modal-lg | modal-xl with scrollable |
| Workflow | Single step | Two-phase (Upload → Review) |
| Error Handling | Generic | Specific per phase |
| Draft Management | N/A | Can discard and retry |

