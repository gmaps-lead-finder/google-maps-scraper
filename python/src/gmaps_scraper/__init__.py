"""Official Python client for the GMaps Lead Finder Agent HTTP API."""

from .client import Client
from .errors import (
    ApiError,
    AuthenticationError,
    BadRequestError,
    InsufficientCreditsError,
    JobConflictError,
    NotFoundError,
    PlanNotAllowedError,
    TimeoutError,
)

__all__ = [
    "Client",
    "ApiError",
    "AuthenticationError",
    "BadRequestError",
    "InsufficientCreditsError",
    "JobConflictError",
    "NotFoundError",
    "PlanNotAllowedError",
    "TimeoutError",
]

__version__ = "0.1.3"
