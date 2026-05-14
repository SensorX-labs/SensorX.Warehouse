# SensorX.Warehouse Project Memory

## Project Overview
- **Project Name:** SensorX.Warehouse
- **Architecture:** Clean Architecture / DDD
- **Tech Stack:** .NET 9, EF Core, Npgsql, MediatR

## Recent Changes (2026-04-23)
- **Pagination System Synchronization:** Synchronized pagination patterns with `SensorX.Data`.
    - Added `OffsetPagination` and `KeysetPagination` in `Application\Common\QueryExtensions`.
    - Removed old `Common\Pagination` directory.

## Pagination System
### 1. Offset Pagination (`OffsetPagination` folder)
- **Use case**: Standard web tables with total page counts.
- **Base Query**: `OffsetPagedQuery` (contains `PageNumber`, `PageSize`).
- **Result Wrapper**: `OffsetPagedResult<T>` (contains `TotalCount`, `TotalPages`, etc.).
- **Extension**: `ApplyOffsetPagination(request)`.

### 2. Keyset Pagination (`KeysetPagination` folder)
- **Use case**: High-performance infinite scroll or large datasets.
- **Base Query**: `KeysetPagedQuery` (contains cursors).
- **Result Wrapper**: `KeysetPagedResult<T>` (contains cursors for next/previous).
- **Extension**: `ApplyKeysetPagination(request, createdAtSelector, idSelector)`.
