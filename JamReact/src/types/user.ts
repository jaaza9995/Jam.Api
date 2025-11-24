export interface User {
    userId: number;
    // subject/ username
    sub: string;
    password: string;
    // (JWT ID):id for JWT to prevenet replay attacks
    jti: string;
    // (Issued At):timestamp when token was issued
    iat: number;
    // (Expiration Time): timestamp when token will expire
    exp: number;
    // (Issuer): identifies authority that issued the token
    iss: string;
    // (Audience): identifies recipients that the token is intended for
    aud: string;
    // (Roles): roles assigned to the user
    role: string | string[];
}


/* 
    What Gemini had to say about this interface:
    ⚠️ NOTE: It is a bit unusual to have userId as a number and sub as a string 
    in the same DTO, and it is very unusual and insecure to have password in a 
    decoded JWT object in the Frontend. If password is not needed for the frontend 
    logic, consider removing it from the interface and ensure it is not sent in the 
    JWT. But for now, focus on role.
*/