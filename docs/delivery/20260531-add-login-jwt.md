# Sign-In & User Authentication — Delivery Summary

The Knights admin system now supports secure user sign-in with industry-standard JWT authentication. Users can sign in with a username or email address and a password; valid credentials grant access to the system for one hour, with invalid attempts rejected uniformly for security. All passwords are encrypted using industry-standard PBKDF2 hashing, and the underlying API and web interface are protected to ensure only signed-in users can access them.

## What's Included

- **Secure password storage**: All user passwords are hashed using PBKDF2, a vetted encryption standard that protects accounts even if the database is compromised.
- **Sign-in token system**: Users receive a cryptographically signed token valid for 60 minutes after successful login; subsequent actions use this token to prove identity.
- **Sign-in page**: A clean login form accepting username or email with a password field, clear error messaging for incorrect credentials, and visual feedback during sign-in.
- **Access control**: Users who are not signed in are automatically redirected to the sign-in page; the system prevents access to admin functions without a valid token.
- **Account creation flow**: New user accounts are created with a real password and can immediately sign in.
- **Automated verification**: Both backend and frontend sign-in logic have been tested to confirm they work correctly.

## How It Works at a Glance

When a user opens the admin system, they see the sign-in page. They enter their username (or email) and password, then click **Sign In**.

The system checks the credentials against stored records. If they match, the user receives a token—a secure digital pass valid for one hour. This token is stored securely on the device and attached to every subsequent action the user takes in the system.

If credentials don't match (wrong password, unknown account, or inactive account), the sign-in fails and the user sees a generic error message with no hint as to which part was wrong. This protects accounts from guessing attacks.

When the token expires after one hour, the user is logged out and sent back to the sign-in page.

## Notes & Limitations

- **Signing secret is placeholder configuration**: The cryptographic key that signs tokens is currently set in configuration. Before deploying to production or staging, this secret must be replaced with a securely managed credential (e.g., from a secrets manager) and rotated regularly.
- **Token scope is identity only**: Tokens confirm who the user is (email, username, user ID) but do not yet carry information about what they can do (roles and permissions). Row-level or screen-level access control based on role is a planned follow-up.
- **No refresh or "remember me"**: Tokens last exactly 60 minutes and do not auto-renew. Users must sign in again when tokens expire; there is no persistent login across sessions.
- **No password self-service**: This delivery does not include password reset, account recovery, or email verification. Users cannot change their own passwords or sign themselves up.

## Next Steps

1. **Rotate the signing secret** before moving the system to any shared environment. Coordinate with your infrastructure or security team to store it in a secrets manager.
2. **Configure user creation**: Determine the workflow for adding new user accounts (admin-only form, bulk import, directory sync, etc.) and assign ownership.
3. **Plan role-based access control**: Design which user roles have access to which screens and functions, then implement that as a follow-up.
4. **Password policy** (optional): Decide whether to enforce password complexity, expiration, or history rules.

The system is ready for testing in a local or development environment. Testing in staging or production requires the signing secret rotation mentioned above.
