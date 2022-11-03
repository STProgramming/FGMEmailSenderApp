import { Component, OnInit } from '@angular/core';
import { RoleService } from '../services/role.service';
import { TokenService } from '../services/token.service';

@Component({
  selector: 'app-workspace',
  templateUrl: './workspace.component.html',
  styleUrls: ['./workspace.component.scss']
})
export class WorkspaceComponent implements OnInit {
  seoClaimed: boolean;
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
    this.seoClaimed = this.roleService.roleRequired('Seo');
    this.fgmClaimed = this.roleService.roleRequired('Employee');
    this.adminClaimed = this.roleService.roleRequired('Admin');
  }
}
