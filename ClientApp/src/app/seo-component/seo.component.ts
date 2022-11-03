import { Component, OnInit } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { BehaviorSubject, Observable } from 'rxjs';
import { CedCountryComponent } from '../ced-country-component/ced-country.component';
import { CedDepartmentComponent } from '../ced-department-component/ced-department.component';
import { Country } from '../interfaces/Country';
import { Department } from '../interfaces/Department';
import { SeoUserService } from '../services-api/seo-user.service';

@Component({
  selector: 'app-seo',
  templateUrl: './seo.component.html',
  styleUrls: ['./seo.component.scss']
})
export class SeoComponent implements OnInit {
  listDepartments: Department[];
  listCountries: Country[];
  panelOpenState = false;
  isEmptyListDep: boolean = true;
  isEmptyListCoun: boolean = true;
  downloadFlagCountries$ = new BehaviorSubject<boolean>(false);
  downloadFlagDepartments$ = new BehaviorSubject<boolean>(false);
  _triggerActionCheckCountries: Observable<boolean> = this.downloadFlagCountries$;
  _triggerActionCheckDepartments: Observable<boolean> = this.downloadFlagDepartments$;
  _triggerReloadCountry: Observable<boolean> = this.cedModalCountry.eventReloadCountry$;
  _triggerReloadDepartment : Observable<boolean> = this.cedModalDepartment.eventReloadDepartment$;
  displayedColumnsCountry: string[] = ['id', 'name', 'language', 'edit', 'delete'];
  displayedColumnsDepartment: string[] = ['id', 'name', 'department code', 'department city', 'country', 'edit', 'delete'];

  constructor(
    private readonly seoService: SeoUserService,
    private readonly modalService: NgbModal,
    private readonly cedModalCountry: CedCountryComponent,
    private readonly cedModalDepartment: CedDepartmentComponent
  ) { }

  ngOnInit(): void {
    this._triggerReloadCountry.subscribe(
      (eventTrigger:any) => {
        if(eventTrigger){
          this.getCountries();
        }
      }
    );
    this._triggerReloadDepartment.subscribe(
      (eventTrigger : any) =>{
        if(eventTrigger){
          this.getDepartments();
        }
      }
    );
    this.getCountries();
    this.getDepartments();
    this._triggerActionCheckCountries.subscribe(
      download =>{
        if(download){
          this.checkLengthCountriesList();
        }
      }
    );
    this._triggerActionCheckDepartments.subscribe(
      download =>{
        if(download){
          this.checkLengthDepartmentList();
        }
      }
    );
  }

  async getCountries(){
    await this.seoService.GetAllCountries().subscribe(
      (data: Country[]) =>{
        this.listCountries = data;
        this.downloadFlagCountries$.next(true);
      }
    );
  }

  async getDepartments(){
    await this.seoService.GetAllDepartments().subscribe(
      (data: Department[]) =>{
        this.listDepartments = data;
        this.downloadFlagDepartments$.next(true);
      }
    );
  }

  checkLengthDepartmentList(){
    if(this.listDepartments.length == 0){
      this.isEmptyListDep = true;
    }
    else{
      this.isEmptyListDep = false;
    }
  }

  checkLengthCountriesList(){
    if(this.listCountries.length == 0){
      this.isEmptyListCoun = true;
    }
    else{
      this.isEmptyListCoun = false;
    }
  }

  goToCedDepartment(actionReq: string, idDep: number | null){
    const modalCEDepartment = this.modalService.open(CedDepartmentComponent);
    modalCEDepartment.componentInstance.actionRequired = actionReq;
    if(actionReq !== 'create'){
      modalCEDepartment.componentInstance.idDepartment = idDep;
    }
  }

  goToCedCountry(actionReq: string, idCoun: number | null){
    const modalCECountry = this.modalService.open(CedCountryComponent);
    modalCECountry.componentInstance.actionRequired = actionReq;
    if(actionReq !== 'create'){
      modalCECountry.componentInstance.idCountry = idCoun;
    }
  }

  getCountryNameById(id: number): any{
    let country = this.listCountries.find(i => i.idCountry === id);
    return country.countryName
  }
}
