export interface Department{
    idDepartment: number;
    nameDepartment: string;
    codeDepartment: string;
    cityDepartment: string;
    fK_IdCountry: number;
}

export interface DepartmentInput{
    DepartmentName: string;
    DepartmentCode: string;
    DepartmentCity: string;
    IdCountry: number;
}