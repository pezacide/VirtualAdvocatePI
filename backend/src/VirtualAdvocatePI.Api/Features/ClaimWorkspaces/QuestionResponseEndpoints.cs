using Microsoft.EntityFrameworkCore;
using VirtualAdvocatePI.Api.Data;
using VirtualAdvocatePI.Api.Domain.Claims;
using VirtualAdvocatePI.Api.Services;

namespace VirtualAdvocatePI.Api.Features.ClaimWorkspaces;

public static class QuestionResponseEndpoints
{
    public static IEndpointRouteBuilder MapQuestionResponseEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/question-responses", async (
            Guid workspaceId,
            Guid conditionId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var responses = await db.QuestionResponses
                .Where(x => x.ClaimWorkspaceId == workspaceId && x.ConditionId == conditionId && x.Status != "ARCHIVED")
                .OrderBy(x => x.QuestionGroup)
                .ThenBy(x => x.QuestionKey)
                .ToListAsync();

            return Results.Ok(responses.Select(ToQuestionResponseResponse).ToList());
        });

        app.MapPost("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/question-responses", async (
            Guid workspaceId,
            Guid conditionId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            CreateQuestionResponseRequest input) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            if (string.IsNullOrWhiteSpace(input.QuestionKey))
            {
                return Results.BadRequest(new { error = "Question key is required." });
            }

            if (string.IsNullOrWhiteSpace(input.QuestionText))
            {
                return Results.BadRequest(new { error = "Question text is required." });
            }

            var questionGroup = NormaliseQuestionGroup(input.QuestionGroup);

            if (!IsValidQuestionGroup(questionGroup))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid question group.",
                    allowedValues = GetAllowedQuestionGroups()
                });
            }

            var answerType = NormaliseAnswerType(input.AnswerType);

            if (!IsValidAnswerType(answerType))
            {
                return Results.BadRequest(new
                {
                    error = "Invalid answer type.",
                    allowedValues = GetAllowedAnswerTypes()
                });
            }

            var response = new QuestionResponse
            {
                ClaimWorkspaceId = workspaceId,
                ConditionId = conditionId,
                QuestionGroup = questionGroup,
                QuestionKey = input.QuestionKey.Trim(),
                QuestionText = input.QuestionText.Trim(),
                AnswerText = input.AnswerText,
                AnswerType = answerType,
                Status = "ACTIVE",
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            db.QuestionResponses.Add(response);

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "QUESTION_RESPONSE_CREATED",
                $"Question response created. ConditionId={conditionId}; QuestionKey={response.QuestionKey}; ResponseId={response.Id}");

            await db.SaveChangesAsync();

            return Results.Created(
                $"/api/v1/claim-workspaces/{workspaceId}/conditions/{conditionId}/question-responses/{response.Id}",
                ToQuestionResponseResponse(response));
        });

        app.MapGet("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/question-responses/{responseId:guid}", async (
            Guid workspaceId,
            Guid conditionId,
            Guid responseId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var response = await db.QuestionResponses
                .FirstOrDefaultAsync(x =>
                    x.Id == responseId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED");

            if (response is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(ToQuestionResponseResponse(response));
        });

        app.MapPatch("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/question-responses/{responseId:guid}", async (
            Guid workspaceId,
            Guid conditionId,
            Guid responseId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db,
            UpdateQuestionResponseRequest input) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var response = await db.QuestionResponses
                .FirstOrDefaultAsync(x =>
                    x.Id == responseId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED");

            if (response is null)
            {
                return Results.NotFound();
            }

            if (!string.IsNullOrWhiteSpace(input.QuestionGroup))
            {
                var questionGroup = NormaliseQuestionGroup(input.QuestionGroup);

                if (!IsValidQuestionGroup(questionGroup))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid question group.",
                        allowedValues = GetAllowedQuestionGroups()
                    });
                }

                response.QuestionGroup = questionGroup;
            }

            if (!string.IsNullOrWhiteSpace(input.QuestionKey))
            {
                response.QuestionKey = input.QuestionKey.Trim();
            }

            if (!string.IsNullOrWhiteSpace(input.QuestionText))
            {
                response.QuestionText = input.QuestionText.Trim();
            }

            if (input.AnswerText is not null)
            {
                response.AnswerText = input.AnswerText;
            }

            if (!string.IsNullOrWhiteSpace(input.AnswerType))
            {
                var answerType = NormaliseAnswerType(input.AnswerType);

                if (!IsValidAnswerType(answerType))
                {
                    return Results.BadRequest(new
                    {
                        error = "Invalid answer type.",
                        allowedValues = GetAllowedAnswerTypes()
                    });
                }

                response.AnswerType = answerType;
            }

            response.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "QUESTION_RESPONSE_UPDATED",
                $"Question response updated. ConditionId={conditionId}; QuestionKey={response.QuestionKey}; ResponseId={response.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(ToQuestionResponseResponse(response));
        });

        app.MapDelete("/api/v1/claim-workspaces/{workspaceId:guid}/conditions/{conditionId:guid}/question-responses/{responseId:guid}", async (
            Guid workspaceId,
            Guid conditionId,
            Guid responseId,
            HttpRequest request,
            CurrentUserService currentUserService,
            ClaimAccessService claimAccessService,
            AuditService auditService,
            VirtualAdvocateDbContext db) =>
        {
            var user = await currentUserService.GetOrCreateCurrentUserAsync(request);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            if (!await claimAccessService.UserOwnsConditionAsync(user.Id, workspaceId, conditionId))
            {
                return Results.NotFound();
            }

            var response = await db.QuestionResponses
                .FirstOrDefaultAsync(x =>
                    x.Id == responseId &&
                    x.ClaimWorkspaceId == workspaceId &&
                    x.ConditionId == conditionId &&
                    x.Status != "ARCHIVED");

            if (response is null)
            {
                return Results.NotFound();
            }

            response.Status = "ARCHIVED";
            response.UpdatedAt = DateTimeOffset.UtcNow;

            auditService.AddAuditEvent(
                request,
                user.Id,
                workspaceId,
                "QUESTION_RESPONSE_ARCHIVED",
                $"Question response archived. ConditionId={conditionId}; QuestionKey={response.QuestionKey}; ResponseId={response.Id}");

            await db.SaveChangesAsync();

            return Results.Ok(new
            {
                id = response.Id,
                status = response.Status,
                archived = true
            });
        });

        return app;
    }

    internal static object ToQuestionResponseResponse(QuestionResponse response)
    {
        return new
        {
            id = response.Id,
            claimWorkspaceId = response.ClaimWorkspaceId,
            conditionId = response.ConditionId,
            questionGroup = response.QuestionGroup,
            questionKey = response.QuestionKey,
            questionText = response.QuestionText,
            answerText = response.AnswerText,
            answerType = response.AnswerType,
            status = response.Status,
            createdAt = response.CreatedAt,
            updatedAt = response.UpdatedAt
        };
    }

    private static string NormaliseQuestionGroup(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "CLAIM_CONTEXT" : value.Trim().ToUpperInvariant();
    }

    private static string NormaliseAnswerType(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "TEXT" : value.Trim().ToUpperInvariant();
    }

    private static bool IsValidQuestionGroup(string value) => GetAllowedQuestionGroups().Contains(value);

    private static bool IsValidAnswerType(string value) => GetAllowedAnswerTypes().Contains(value);

    private static string[] GetAllowedQuestionGroups()
    {
        return new[]
        {
            "CLAIM_CONTEXT",
            "DIAGNOSIS",
            "SYMPTOMS",
            "TREATMENT",
            "MEDICATION",
            "FUNCTIONAL_IMPACT",
            "LIFESTYLE_IMPACT",
            "WORK_IMPACT",
            "STABILITY",
            "WORSENING",
            "PREVIOUS_COMPENSATION",
            "EVIDENCE_AVAILABLE",
            "EVIDENCE_MISSING",
            "DOCTOR_QUESTIONS"
        };
    }

    private static string[] GetAllowedAnswerTypes()
    {
        return new[]
        {
            "TEXT",
            "LONG_TEXT",
            "YES_NO",
            "YES_NO_UNSURE",
            "DATE",
            "MULTI_SELECT",
            "SINGLE_SELECT",
            "FILE_REFERENCE"
        };
    }
}

public sealed record CreateQuestionResponseRequest(
    string? QuestionGroup,
    string? QuestionKey,
    string? QuestionText,
    string? AnswerText,
    string? AnswerType
);

public sealed record UpdateQuestionResponseRequest(
    string? QuestionGroup,
    string? QuestionKey,
    string? QuestionText,
    string? AnswerText,
    string? AnswerType
);
