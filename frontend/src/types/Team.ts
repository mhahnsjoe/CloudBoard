export interface Team {
  id: number
  name: string
  description?: string
  createdAt: string
  memberCount: number
  projectCount: number
  currentUserRole: TeamRole
}

export interface TeamDetail {
  id: number
  name: string
  description?: string
  createdAt: string
  createdBy: UserSummary
  members: TeamMember[]
  projects: ProjectSummary[]
  pendingInvitations: TeamInvitation[]
  currentUserRole: TeamRole
}

export interface TeamMember {
  userId: number
  name: string
  email: string
  role: TeamRole
  joinedAt: string
}

export interface TeamInvitation {
  id: number
  email: string
  role: TeamRole
  createdAt: string
  expiresAt: string
  invitedBy: UserSummary
  isExpired: boolean
  token?: string
}

export interface UserSummary {
  id: number
  name: string
  email: string
}

export interface ProjectSummary {
  id: number
  name: string
  createdAt: string
}

export type TeamRole = 'Member' | 'Admin' | 'Owner'

// Request DTOs
export interface CreateTeamDto {
  name: string
  description?: string
}

export interface UpdateTeamDto {
  name?: string
  description?: string
}

export interface InviteMemberDto {
  email: string
  role: TeamRole
}

export interface UpdateMemberRoleDto {
  role: TeamRole
}

export interface AcceptInvitationDto {
  token: string
}

// Invitation from the invited user's perspective (includes team info)
export interface MyInvitation {
  id: number
  teamId: number
  teamName: string
  teamDescription?: string
  role: TeamRole
  createdAt: string
  expiresAt: string
  invitedBy: UserSummary
  token: string
}
