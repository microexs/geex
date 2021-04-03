﻿using System.Threading;
using System.Threading.Tasks;
using Geex.Core.Authentication.Domain.Events;
using MediatR;

namespace Geex.Core.Authorization.Handlers
{
    public class UpdateCasbinOnUserRoleChanged : INotificationHandler<UserRoleChangedEvent>
    {
        public UpdateCasbinOnUserRoleChanged(RbacEnforcer enforcer)
        {
            Enforcer = enforcer;
        }

        public RbacEnforcer Enforcer { get; init; }
        public async Task Handle(UserRoleChangedEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() => Enforcer.SetRolesForUser(notification.UserId, notification.Roles), cancellationToken);
        }
    }
}