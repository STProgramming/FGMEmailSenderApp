import { Component, Input, OnInit } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { BehaviorSubject, Observable } from 'rxjs';
import { ErrorModalComponent } from '../error-modal-component/error-modal.component';
import { Country, CountryInput } from '../interfaces/Country';
import { SeoUserService } from '../services-api/seo-user.service';

@Component({
  selector: 'app-ce-country',
  templateUrl: './ced-country.component.html',
  styleUrls: ['./ced-country.component.scss']
})
export class CedCountryComponent implements OnInit {
  @Input() actionRequired: string;
  @Input() idCountry: number | null = null;
  country: Country;
  formCountry: FormGroup = null;
  dowloadedCountriesList$ = new BehaviorSubject<boolean>(false);
  _observableDownload: Observable<boolean> = this.dowloadedCountriesList$
  loading: boolean = false;
  eventReloadCountry$ = new BehaviorSubject<boolean>(false);

  constructor(private readonly seoServices: SeoUserService,
    private readonly modalService: NgbModal,
    private readonly router: Router) {}

  ngOnInit(): void {
    this.formCountry = new FormGroup({
      name: new FormControl('', Validators.required),
      language: new FormControl('', Validators.required)
    });
    if(this.actionRequired === 'edit' && this.idCountry != null){
      this.loading = true;
      this.getCountryDetails();
      this._observableDownload.subscribe(
        (download: any) =>{
          if(download){
            this.fetchFormDetails();
            this.loading = false;
          }
        }
      );
    }
  }

  getCountryDetails(){
    this.seoServices.GetCountryDetails(this.idCountry).subscribe(
      (data: any) => {
        if(data){
          this.country = data;
          this.dowloadedCountriesList$.next(true);
        }
      }
    );
  }

  fetchFormDetails(){
    this.formCountry = new FormGroup({
      name: new FormControl(this.country.countryName, Validators.required),
      language: new FormControl(this.country.languageCountry, Validators.required),
    });
  }

  sendingData(formCoun: FormGroup){
    if(formCoun.valid){
      let name = formCoun.get('name').value;
      let language = formCoun.get('language').value;
      let newCountry: CountryInput = {
        "CountryName": name,
        "CountryLanguage": language,
      };
      if(this.actionRequired === 'create'){
        this.seoServices.CreateNewCountry(newCountry).subscribe();
        this.onEndRequest();
      }
      else{
        this.seoServices.EditCountry(newCountry, this.idCountry).subscribe();
        this.onEndRequest();
      }
    }
  }

  deleteCountry(){
    this.seoServices.DeleteCountry(this.idCountry).subscribe();
    this.onEndRequest();
  }

  refuseDelete(){
    this.modalService.dismissAll();
  }

  onEndRequest(){
    this.eventReloadCountry$.next(true);
    this.modalService.dismissAll();
    this.eventReloadCountry$.next(false);
  }
}
