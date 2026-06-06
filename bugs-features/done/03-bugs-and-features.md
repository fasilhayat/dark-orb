# Bug Report - Unhandled API Error Causes Application Crash During Combat Initialization

Project: Dark Orb

## Severity

Critical

## Status

Open

---

# Summary

The application crashes completely when attempting to start certain combat combinations.

The crash occurs because an HTTP 400 (Bad Request) response from the API is not handled gracefully by the GUI.

Instead of displaying an error message and allowing the user to continue using the application, the exception propagates to the top level and terminates the entire process.

This is both a stability issue and an error-handling defect.

---

# Problem Description

When selecting certain combatants and starting combat, the API returns a Bad Request response.

The GUI calls:

```csharp
HttpResponseMessage.EnsureSuccessStatusCode()
```

which immediately throws an exception.

The exception is not caught by the combat startup workflow.

As a result:

* Combat initialization fails.
* The exception propagates through the UI.
* Avalonia terminates the application.
* The user loses the current session.
* No useful error information is displayed.

An API validation failure should never be capable of crashing the client application.

---

# Steps To Reproduce

### Scenario

1. Launch the application.
2. Select an API Version.
3. Select Duel mode.
4. Select Golem.
5. Select Sister Elira Vane.
6. Click Proceed.

---

# Actual Result

The combat screen appears briefly.

The application then crashes.

The following exception is thrown:

```text
System.Net.Http.HttpRequestException:
Response status code does not indicate success:
400 (Bad Request)
```

Stack trace indicates failure in:

```text
BattleArenaApiClient.SimulateCombatAsync()
```

Specifically:

```text
BattleArenaApiClient.cs:line 122
```

followed by propagation through:

```text
MainWindow.RunCombat()
MainWindow.StartCombat()
MainWindow.OnFightClick()
```

Eventually causing process termination.

---

# Expected Result

If the API returns HTTP 400:

* The application must remain running.
* The error must be handled gracefully.
* The user must receive a meaningful error message.
* Combat initialization should abort safely.
* The application should return to a usable state.

The GUI must never crash because an API endpoint returned a validation error.

---

# Technical Analysis

The immediate crash originates from:

```csharp
response.EnsureSuccessStatusCode();
```

which throws:

```csharp
HttpRequestException
```

for all non-success HTTP status codes.

The exception appears to be uncaught.

This causes application termination.

---

# Required Fix

## Error Handling

Wrap combat startup API calls in proper exception handling.

Handle:

* HttpRequestException
* TaskCanceledException
* TimeoutException
* JsonException
* InvalidOperationException
* Unexpected exceptions

The GUI should remain functional regardless of API failures.

---

## User Feedback

Display a user-friendly error dialog containing:

* Error title
* User-readable explanation
* Technical details (optional)

Example:

```text
Unable to start combat.

The selected combat configuration was rejected by the API.

Please verify the selected characters and try again.
```

---

## API Error Diagnostics

If the API returns:

```http
400 Bad Request
```

capture and log:

* Response status code
* Response body
* Validation messages
* Request payload

before presenting the error to the user.

Current logs only reveal the HTTP status code.

The actual API validation error is hidden.

---

## Root Cause Investigation

The underlying API failure must also be investigated.

Determine why:

```text
Golem
vs
Sister Elira Vane
```

produces a 400 response.

Potential causes include:

* Invalid character configuration.
* Missing abilities.
* Invalid equipment.
* Invalid combat payload.
* Serialization issue.
* Null values.
* Character data corruption.
* API validation defect.

The actual API response body should identify the root cause.

---

# Acceptance Criteria

### Scenario 1

**Given** the API returns HTTP 400

**When** combat initialization occurs

**Then** the application remains running

**And** the error is displayed to the user

**And** no crash occurs

---

### Scenario 2

**Given** the API returns validation details

**When** combat initialization fails

**Then** validation details are logged

**And** developers can identify the root cause

---

### Scenario 3

**Given** combat initialization fails

**When** the error dialog is closed

**Then** the user can continue using the application

**And** another combat can be started

---

### Scenario 4

**Given** any API failure occurs

**When** the failure is encountered

**Then** the application remains stable

**And** no unhandled exception reaches the application boundary

---

# Reqnroll Regression Test Requirements

## Scenario - API Returns Bad Request

**Given** the combat API returns HTTP 400

**When** combat initialization is requested

**Then** the application should remain running

**And** an error should be shown to the user

**And** combat should not start

---

## Scenario - Invalid Combat Configuration

**Given** a combat configuration rejected by the API

**When** the user clicks Proceed

**Then** the application should remain stable

**And** validation information should be logged

**And** the user should be informed

---

## Scenario - Recover After Failure

**Given** a combat initialization failure occurred

**When** the user starts a different valid combat

**Then** combat should start successfully

**And** no stale error state should remain

---

# Unit Test Requirements

Create tests verifying:

* HTTP 400 responses are handled correctly.
* API validation messages are captured.
* Error dialogs are shown.
* Combat startup aborts safely.
* Unhandled exceptions cannot terminate combat initialization.
* Recovery is possible after a failed combat startup.

---

# Validation Requirements

* [ ] Reproduce the crash using Golem vs Sister Elira Vane.
* [ ] Capture complete HTTP response body.
* [ ] Identify actual API validation error.
* [ ] Verify application no longer crashes.
* [ ] Verify user-friendly error message is displayed.
* [ ] Verify error details are logged.
* [ ] Verify user can start another combat afterward.
* [ ] Verify unit tests pass.
* [ ] Verify Reqnroll regression tests pass.
* [ ] Verify no unhandled exceptions terminate the GUI.

## Deliverables

1. Root cause analysis of the HTTP 400 response.
2. Root cause analysis of the GUI crash.
3. Captured API validation response.
4. Description of implemented exception handling.
5. List of affected files.
6. Unit test results.
7. Reqnroll regression test results.
8. Confirmation that API failures can no longer crash the application.
9. Confirmation that users can recover from combat startup failures without restarting the application.
