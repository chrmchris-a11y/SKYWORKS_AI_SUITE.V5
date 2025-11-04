# OSO Comments Management System

## Overview
The OSO Comments System provides a comprehensive tracking mechanism for feedback and observations across Operational Safety Objectives (OSO) in both SORA 2.0 and 2.5 versions.

## SORA Version Support
- **SORA 2.0:** Supports 7 OSOs (OSO-10, OSO-11, OSO-12, OSO-14, OSO-15, OSO-21, OSO-22)
- **SORA 2.5:** Supports 3 OSOs (OSO-11, OSO-17, OSO-23)

## API Endpoints

### 1. Add Comment
```
POST /api/oso-comments
```
**Request Body:**
```json
{
  "osoId": "OSO-11",
  "soraVersion": "2.5",
  "content": "Suggestion for improvement",
  "author": "Safety Analyst",
  "priority": "Medium"
}
```

### 2. Get Comments for Specific OSO
```
GET /api/oso-comments/{osoId}/{soraVersion}
```

### 3. Update Comment Status
```
PUT /api/oso-comments/{commentId}/status
```
**Request Body:**
```json
{
  "status": "InProgress"
}
```

### 4. Get Unresolved Comments
```
GET /api/oso-comments/unresolved
```

## Comment Lifecycle
1. **Open:** Initial state of a new comment
2. **In Progress:** Under review
3. **Resolved:** Issues addressed
4. **Closed:** Final state

## Status Codes
- `201 Created`: Successful comment addition
- `400 Bad Request`: Invalid input
- `404 Not Found`: Comment not found

## Best Practices
- Always specify SORA version
- Provide clear, concise comments
- Use appropriate priority levels
- Keep comments professional and constructive

**Version:** 1.0
**Last Updated:** 2024-03-15
**Reference:** EASA SORA 2.0 & 2.5 Guidelines