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
}