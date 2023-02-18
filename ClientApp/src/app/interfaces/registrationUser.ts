export interface RegistrationUser{
    Name: string;
    LastName: string;
    Email: string;
    PhoneNumber: string;
    Password: string;
}

export interface confirmEmailInputModel{
    token:string,
    email:string
}