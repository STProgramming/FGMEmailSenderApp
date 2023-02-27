import { Component, OnInit } from '@angular/core';
import { RoleService } from '../services/role.service';
import { TokenService } from '../services/token.service';

@Component({
  selector: 'app-workspace',
  templateUrl: './workspace.component.html',
  styleUrls: ['./workspace.component.scss']
})
export class WorkspaceComponent implements OnInit {
  referentClaimed: boolean;
  adminClaimed: boolean;
  fgmClaimed: boolean;
  time = new Date();
  intervalId: any;
  nameUser : string;

  constructor(
    private readonly roleService: RoleService,
    private readonly tokenService: TokenService
  ) { }

  ngOnInit(): void {
    this.nameUser = this.tokenService.getNameUserToken();
    this.intervalId = setInterval(() => {
      this.time = new Date();
    }, 1000);
    this.referentClaimed = this.roleService.roleRequired('Referent');
    this.fgmClaimed = this.roleService.roleRequired('FGMEmployee');
    this.adminClaimed = this.roleService.roleRequired('Administrator');
  }
}
