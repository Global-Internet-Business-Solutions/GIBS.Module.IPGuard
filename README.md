# GIBS.Module.IPGuard

GIBS.Module.IPGuard is an Oqtane module for automatically and manually blocking IP addresses based on repeated 404 activity.

## Overview

The module adds a server-side middleware component that inspects incoming requests, checks whether the caller IP address is already blocked, and records qualifying 404 requests. When an IP exceeds the configured threshold within the configured time window, the module adds that IP address to the blocked list.

The module also includes an administrative dashboard for reviewing blocked IPs and recent 404 attempts.

## Core Features

### Automatic IP blocking

- Records qualifying 404 requests by client IP address.
- Counts recent 404 attempts inside a configurable block window.
- Automatically blocks an IP when its recent attempts meet or exceed the configured threshold.
- Uses a unique index on blocked IP addresses so each IP can only exist once in the blocked table.
- Handles concurrent block attempts safely when multiple requests cross the threshold at the same time.

### Request-time enforcement

- Blocks requests from IPs already present in the blocked list.
- Returns HTTP `403 Forbidden` with `Access denied.` for blocked IPs.
- Reads `X-Forwarded-For` first, then falls back to `HttpContext.Connection.RemoteIpAddress`.
- Skips loopback addresses.

### 404 tracking safeguards

- Deduplicates 404 recording within a single request pipeline execution.
- Tracks the original request path and query string when status-code re-execution occurs.
- Ignores framework/static paths such as:
  - `/_blazor`
  - `/_framework`
  - `/_content`
  - `/_vs`
- Optionally excludes Oqtane reserved or internal routes such as `/admin` and other reserved segments.
- Tracks extensionless routes only.

### Manual blocked IP management

Administrators can:

- Add a blocked IP manually.
- Edit an existing blocked IP.
- Unblock an IP.
- View block metadata including:
  - IP address
  - country lookup
  - error count
  - blocked date
  - reason
  - audit information

### Recent 404 attempt management

Administrators can:

- View recent 404 attempts.
- Refresh the blocked IP and 404 attempt lists.
- Delete all 404 attempts.
- Delete 404 attempts for a specific IP address.
- Move an IP from the recent 404 list directly into the blocked list using **Add To Blocked**.

When **Add To Blocked** is used:

- The IP is added to the blocked list if it does not already exist.
- Country lookup is performed automatically.
- `ErrorCount` is set to the total recorded attempts for that IP.
- `Reason` is set to `Administrator manual block.`
- Existing blocked records are updated if needed.
- All 404-attempt rows for that IP are removed from the recent attempts table.

### Country lookup

- Looks up country information for auto-blocked IPs.
- Looks up country information for manually added recent-404 blocks.
- Preserves or refreshes missing country data when blocked records are updated.

## Settings

The module includes the following settings:

- **Enabled**  
  Turns automatic IP blocking on or off.

- **404 Threshold**  
  Number of 404 attempts required before an IP is blocked.

- **Block Window (Hours)**  
  Number of hours to look back when counting recent 404 attempts.

- **Allowed IP Addresses**  
  Comma-separated list of IP addresses that should never be blocked.

- **Exclude Reserved Routes**  
  Prevents Oqtane reserved and internal routes from being counted as 404 attempts.

- **Diagnostic Mode**  
  Enables additional diagnostic logging for threshold crossing and concurrency troubleshooting.

## Multi-tenant behavior

- Middleware runs for incoming site requests and evaluates the active alias/site context.
- Blocked IP and 404-attempt storage is global within the tenant database.
- The blocked IP list is not site-scoped.
- Host settings are preferred, with fallback behavior available for legacy or transition scenarios.

## Reliability and runtime behavior

- Normalizes IP addresses before persistence and comparison.
- Cleans up 404-attempt rows after an IP becomes blocked.
- Cleans up leftover 404 attempts when a blocked IP is encountered again.
- Safely no-ops during tenant-installation or tenant-initialization gaps where persistence is not yet ready.
- Logs initialization bypasses as warnings instead of breaking the request pipeline.

## Administrative UI

### Blocked IP Addresses tab

Displays:

- IP address
- Country lookup
- Error count
- Blocked date
- Reason

Actions:

- Refresh
- Block IP
- Edit
- Unblock

### Recent 404 Attempts tab

Displays:

- IP address
- Requested URL
- Attempt date
- Block action button

Actions:

- Refresh
- Delete By IP
- Delete All
- Add To Blocked

## Data stored

### Blocked IPs

Each blocked IP record stores:

- IP address
- country lookup
- error count
- blocked date
- reason
- audit fields inherited from Oqtane model base

### Error attempts

Each 404-attempt record stores:

- IP address
- requested URL
- attempt date
- audit fields inherited from Oqtane model base

## Typical workflow

1. Enable the module.
2. Configure the threshold and block window.
3. Optionally define allowed IP addresses.
4. Optionally exclude reserved/internal routes.
5. Review recent 404 attempts.
6. Allow the middleware to auto-block offenders or manually block them from the dashboard.
7. Unblock or edit entries as needed.

## Version

Current module version: `1.0.9`
