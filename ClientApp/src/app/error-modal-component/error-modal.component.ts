import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-error-modal',
  templateUrl: './error-modal.component.html',
  styleUrls: ['./error-modal.component.scss']
})
export class ErrorModalComponent implements OnInit {
  @Input() public errorMessage: any;
  title: string = 'Abbiamo riscontrato un errore nella sua richiesta';
  
  constructor(
    private readonly router: Router
  ) { }

  ngOnInit(): void {
  }

  onOkPress(){
    //questa funzione ha il dovere di reindirizzare l'utente nell'index dell'applicazione
    this.router.navigateByUrl('');
  }
}
