# Plan for Controller Profile Editing Dialog Implementation

## Overview
This document outlines the approach for implementing the "Implement profile editing dialog" TODO in the ControllerPage. The feature will allow users to edit existing Dolphin controller profiles with a comprehensive and user-friendly interface.

## Current State Analysis

### Existing Infrastructure
1. **DolphinControllerService**: Handles profile CRUD operations, mapping management, and Dolphin integration
2. **DolphinControllerProfile**: Contains profile name, controller type, mapping, creation date, and active status
3. **DolphinControllerMapping**: Stores button mappings as key-value pairs (Dolphin button → Input mapping)
4. **Popup System**: Well-established popup infrastructure using `PopupContent` base class
5. **UI Components**: Consistent button styles, form components, and design patterns

### Current TODO Location
- **File**: `WheelWizard/Views/Pages/ControllerPage.axaml.cs`
- **Method**: `ShowEditProfileDialog(string profileName)` (line ~680)
- **Current Implementation**: Placeholder that shows a message dialog

## Implementation Plan

### 1. Create Profile Editing Dialog Structure

#### 1.1 Dialog Files to Create
- `WheelWizard/Views/Popups/ControllerManagement/ProfileEditWindow.axaml`
- `WheelWizard/Views/Popups/ControllerManagement/ProfileEditWindow.axaml.cs`
- `WheelWizard/Views/Popups/ControllerManagement/ButtonMappingEditor.axaml`
- `WheelWizard/Views/Popups/ControllerManagement/ButtonMappingEditor.axaml.cs`

#### 1.2 Dialog Features
- **Profile Information Section**:
  - Profile name (editable text field)
  - Controller type (read-only display)
  - Creation date (read-only display)
  - Active status (toggle switch)

- **Button Mapping Section**:
  - Visual controller layout with clickable buttons
  - Real-time button press detection
  - Mapping assignment interface
  - Reset to defaults option

- **Action Buttons**:
  - Save changes
  - Cancel (discard changes)
  - Reset mappings
  - Test mappings

### 2. Dialog Design and Layout

#### 2.1 Main Dialog Structure
```
┌─────────────────────────────────────────────────────────┐
│ Profile Editor - [Profile Name]                    [×] │
├─────────────────────────────────────────────────────────┤
│ Profile Information:                                    │
│ ┌─────────────────┐ ┌─────────────────┐                │
│ │ Name: [______]  │ │ Type: Xbox      │                │
│ │ Created: Date   │ │ Active: [✓]     │                │
│ └─────────────────┘ └─────────────────┘                │
│                                                         │
│ Button Mappings:                                        │
│ ┌─────────────────────────────────────────────────────┐ │
│ │ [Visual Controller Layout]                          │ │
│ │                                                     │ │
│ │ Face Buttons:  [A] [B] [X] [Y]                     │ │
│ │ Shoulders:     [L] [R]                             │ │
│ │ D-Pad:         [↑] [↓] [←] [→]                     │ │
│ │ Sticks:        [LS] [RS]                           │ │
│ │ Triggers:      [LT] [RT]                           │ │
│ │ Start/Back:    [Start] [Back]                      │ │
│ └─────────────────────────────────────────────────────┘ │
│                                                         │
│ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐        │
│ │   Reset     │ │    Test     │ │    Save     │        │
│ └─────────────┘ └─────────────┘ └─────────────┘        │
└─────────────────────────────────────────────────────────┘
```

#### 2.2 Visual Controller Layout
- **Interactive Controller Visualization**: SVG-based controller layout
- **Button States**: Visual feedback for pressed/unpressed states
- **Mapping Display**: Show current mappings on buttons
- **Click-to-Edit**: Click any button to assign new mapping

### 3. Implementation Details

#### 3.1 ProfileEditWindow Class
```csharp
public partial class ProfileEditWindow : PopupContent
{
    private DolphinControllerProfile _originalProfile;
    private DolphinControllerProfile _editingProfile;
    private bool _hasChanges = false;
    
    // Properties for data binding
    public string ProfileName { get; set; }
    public ControllerType ControllerType { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    
    // Methods
    public async Task<bool> ShowDialog(string profileName)
    public void SaveChanges()
    public void ResetMappings()
    public void TestMappings()
}
```

#### 3.2 Button Mapping Editor
```csharp
public partial class ButtonMappingEditor : UserControl
{
    private Dictionary<string, string> _currentMappings;
    private ControllerService _controllerService;
    
    // Methods
    public void LoadMappings(Dictionary<string, string> mappings)
    public Dictionary<string, string> GetMappings()
    public void ResetToDefaults()
    public void StartButtonDetection(string dolphinButton)
    public void StopButtonDetection()
}
```

#### 3.3 Integration with Existing Services
- **DolphinControllerService**: Use existing methods for profile operations
- **ControllerService**: Use for real-time button detection during mapping
- **Dependency Injection**: Follow existing patterns with `[Inject]` attributes

### 4. User Experience Flow

#### 4.1 Opening the Dialog
1. User clicks "Edit" button on a profile in ControllerPage
2. `ShowEditProfileDialog(profileName)` is called
3. Profile data is loaded from DolphinControllerService
4. Dialog opens with current profile data

#### 4.2 Editing Process
1. **Profile Information**: User can edit profile name and toggle active status
2. **Button Mapping**: 
   - Click any button on visual controller
   - Press desired input on physical controller
   - Mapping is assigned and displayed
   - Real-time feedback during assignment
3. **Testing**: User can test current mappings without saving

#### 4.3 Saving Changes
1. **Validation**: Ensure profile name is not empty and unique
2. **Confirmation**: Show confirmation dialog if significant changes made
3. **Save**: Update profile in DolphinControllerService
4. **Refresh**: Update ControllerPage profile list
5. **Feedback**: Show success/error message

### 5. Technical Implementation

#### 5.1 Data Management
- **Copy-on-Write**: Work with copy of profile to allow cancellation
- **Change Tracking**: Track modifications to enable/disable save button
- **Validation**: Real-time validation of profile name and mappings

#### 5.2 Real-time Button Detection
- **Timer-based Polling**: Use existing 60fps timer from ControllerPage
- **Button State Tracking**: Track button press/release events
- **Visual Feedback**: Highlight buttons when pressed during mapping

#### 5.3 Error Handling
- **File I/O Errors**: Handle Dolphin config file access issues
- **Validation Errors**: Show specific error messages for invalid inputs
- **Service Errors**: Graceful handling of service failures

### 6. UI/UX Considerations

#### 6.1 Design Consistency
- **Color Scheme**: Use existing neutral/p-primary color palette
- **Typography**: Follow existing text styles (TitleText, BodyText)
- **Spacing**: Use existing margin/padding constants
- **Icons**: Use existing icon set for buttons and indicators

#### 6.2 Accessibility
- **Keyboard Navigation**: Full keyboard support for all controls
- **Screen Readers**: Proper ARIA labels and descriptions
- **High Contrast**: Ensure visibility in high contrast mode
- **Focus Management**: Clear focus indicators and logical tab order

#### 6.3 Responsive Design
- **Minimum Size**: Ensure dialog works on smaller screens
- **Scrolling**: Handle overflow content gracefully
- **Layout**: Adapt layout for different aspect ratios

### 7. Testing Strategy

#### 7.1 Unit Tests
- **ProfileEditWindow**: Test dialog initialization and data binding
- **ButtonMappingEditor**: Test mapping assignment and validation
- **Integration**: Test with DolphinControllerService

#### 7.2 Manual Testing
- **Controller Detection**: Test with various controller types
- **Mapping Assignment**: Test all button types and edge cases
- **Save/Load**: Test profile persistence and loading
- **Error Scenarios**: Test validation and error handling

### 8. Implementation Phases

#### Phase 1: Basic Dialog Structure
1. Create ProfileEditWindow with basic layout
2. Implement profile information editing
3. Add basic save/cancel functionality

#### Phase 2: Button Mapping Interface
1. Create visual controller layout
2. Implement button click detection
3. Add mapping assignment logic

#### Phase 3: Real-time Detection
1. Integrate with ControllerService
2. Add real-time button feedback
3. Implement mapping testing

#### Phase 4: Polish and Testing
1. Add validation and error handling
2. Implement accessibility features
3. Comprehensive testing and bug fixes

### 9. Success Criteria

#### 9.1 Functional Requirements
- [ ] Users can edit profile names
- [ ] Users can toggle active status
- [ ] Users can assign button mappings
- [ ] Users can test mappings
- [ ] Users can reset to defaults
- [ ] Changes are saved to Dolphin configuration

#### 9.2 Quality Requirements
- [ ] Dialog follows existing UI patterns
- [ ] Real-time button detection works reliably
- [ ] Error handling is comprehensive
- [ ] Performance is smooth (60fps)
- [ ] Accessibility standards are met

#### 9.3 User Experience Requirements
- [ ] Intuitive and easy to use
- [ ] Clear visual feedback
- [ ] Responsive to user input
- [ ] Consistent with application design
- [ ] Provides helpful error messages

## Conclusion

This implementation plan provides a comprehensive approach to creating a professional, user-friendly profile editing dialog that integrates seamlessly with the existing WheelWizard codebase. The feature will enhance the controller management experience while maintaining consistency with the application's design patterns and technical architecture. 