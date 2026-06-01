TENANT DATA ISOLATION FEATURE DELIVERY

Delivery Date: June 1, 2026

The Knights platform has successfully implemented query-level tenant data isolation, completing a critical security requirement for multi-tenant operations. This feature ensures that every data query automatically filters results to the requesting tenant's scope, preventing any unauthorized cross-tenant data access. The isolation is enforced at the database query layer through Entity Framework global query filters, eliminating the need for changes to existing API endpoints while providing comprehensive protection across all queries.

The technical implementation introduces a clean abstraction layer for tenant context identification. The ITenantContext interface provides a single point of integration for identifying the current tenant from each request. The HttpTenantContext implementation reads the tenant_id JWT claim embedded in authenticated requests, establishing the tenant scope for that request lifecycle. This design isolates tenant identification logic from the rest of the application, enabling straightforward testing and future authentication mechanism changes without affecting downstream code.

Database-level isolation is enforced within KnightsDbContext using global query filters that automatically scope all User queries to the calling tenant. System administrators retain full cross-tenant visibility when needed for platform operations, addressing both security and operational requirements. The implementation requires no changes to existing API endpoints; all queries automatically benefit from the isolation filters without modifying controller or service layer code.

Comprehensive automated testing validates the isolation guarantees across multiple scenarios. Seventeen test cases confirm that tenant users see only their own user records, system administrators receive full cross-tenant visibility, and the JWT claim parser correctly handles edge cases and malformed input.

This feature closes the multi-tenancy isolation gap that existed since the platform's initial release. Tenant users can no longer access another tenant's user records through any API endpoint, eliminating a significant security vulnerability in production. Combined with the existing role-based and permission-based access controls, tenant data isolation establishes a defense-in-depth approach to protecting sensitive customer data.
