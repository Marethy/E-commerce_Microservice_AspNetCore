"""JWT Token Helper for extracting user information"""
import jwt
from typing import Optional, Dict, Any

def decode_token_without_verification(token: str) -> Optional[Dict[str, Any]]:
    """
    Decode JWT token without signature verification (for extracting claims)
    WARNING: This should only be used after the token has been validated by the backend
    """
    try:
        # Remove 'Bearer ' prefix if present
        if token.startswith("Bearer "):
            token = token[7:]
        
        # Decode without verification (backend already validated it)
        decoded = jwt.decode(token, options={"verify_signature": False})
        return decoded
    except Exception as e:
        print(f"Error decoding token: {e}")
        return None

def extract_username(token: str) -> Optional[str]:
    """Extract username from JWT token"""
    decoded = decode_token_without_verification(token)
    if not decoded:
        return None
    
    # Try different possible username claims
    # IdentityServer4 typically uses 'sub' or 'name' or 'unique_name'
    username = (
        decoded.get("sub") or 
        decoded.get("name") or 
        decoded.get("unique_name") or
        decoded.get("preferred_username") or
        decoded.get("email")
    )
    
    return username

def extract_user_id(token: str) -> Optional[str]:
    """Extract user ID from JWT token"""
    decoded = decode_token_without_verification(token)
    if not decoded:
        return None
    
    return decoded.get("sub") or decoded.get("uid")

def extract_email(token: str) -> Optional[str]:
    """Extract email from JWT token"""
    decoded = decode_token_without_verification(token)
    if not decoded:
        return None
    
    return decoded.get("email")

def get_token_claims(token: str) -> Optional[Dict[str, Any]]:
    """Get all claims from token"""
    return decode_token_without_verification(token)
