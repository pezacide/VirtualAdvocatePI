# Google Cloud Project Structure Decision

## App

Virtual Advocate PI

## Current issue

Creating a new Google Cloud project failed because the active Google account has exceeded its project creation quota.

## Plan B decision

Virtual Advocate PI will use one existing Google Cloud project for the MVP foundation:

dva-sop-dev

## Logical environments

The MVP will separate resources inside the shared project using prefixes:

dev  = vapi-dev
test = vapi-test
prod = vapi-prod

## Reason

This keeps the build moving without waiting for Google Cloud project quota to reset or be increased.

## Safety rule

Do not store real veteran health, claim, identity, or evidence data in this shared MVP project until privacy, consent, access control, deletion, audit logging, and storage security controls are ready.

## Later production decision

Before public production launch, request a Google Cloud project quota increase and split Virtual Advocate PI into separate dev, test, and prod Google Cloud projects.

## Next task

Enable core Google Cloud services.
