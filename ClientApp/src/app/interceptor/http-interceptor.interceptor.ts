import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { ErrorService } from '../services/error.service';
import { environment } from 'src/environments/environment';

@Injectable()
export class HttpInterceptorInterceptor implements HttpInterceptor {
  public APIKEY = environment._APIKEY;

  constructor(private errorService: ErrorService) {}

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> { 
    request = request.clone({
      setHeaders: {
        ApiKey: this.APIKEY
      }
    });   
    return next.handle(request).pipe(catchError((error: HttpErrorResponse) => {
      this.errorService.openErrorModal(error);
      return throwError(error);
    }));
  }
}
