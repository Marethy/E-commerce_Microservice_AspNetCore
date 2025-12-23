import jwt
from typing import Optional, Dict, Any

def decode_token_without_verification(token: str) -> Optional[Dict[str, Any]]:
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
    
    username = (
        decoded.get("sub") or 
        decoded.get("name") or 
        decoded.get("unique_name") or
        decoded.get("preferred_username") or
        decoded.get("email")
    )
    
    return username

def extract_user_id(token: str) -> Optional[str]:
    decoded = decode_token_without_verification(token)
    if not decoded:
        return None
    
    # Log để debug
    import logging
    logger = logging.getLogger(__name__)
    logger.info(f"JWT payload: {decoded}")
    
    return decoded.get("sub") or decoded.get("uid") or decoded.get("id")

def extract_email(token: str) -> Optional[str]:
    decoded = decode_token_without_verification(token)
    if not decoded:
        return None
    
    return decoded.get("email")

def get_token_claims(token: str) -> Optional[Dict[str, Any]]:
    return decode_token_without_verification(token)
