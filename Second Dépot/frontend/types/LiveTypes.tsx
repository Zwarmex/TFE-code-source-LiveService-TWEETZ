export type LiveItemType = {
    id: string
    title: string
    streamer: string
    viewers: number
    thumbnail: string
    avatar: string
}

export type Live = {
    apiVideoLiveStreamId: string
    streamerUsername?: string
    streamerId: string
    title: string
    startTime: Date
    playerUrl: string
    description?: string
    endTime: Date
    streamUrl?: string
    streamKey?: string
    isPublic?: boolean
    invitedUserId?: number | null
    thumbnailUrl?: string | null
    views?: number
    viewerCount?: number
}

export type ProgrammedLiveModifyProps = {
    isOpen: boolean
    liveId: string | null
    initialTitle?: string
    initialIsPublic?: boolean
    initialThumbnailUrl?: string | null
    initialInvitedUserId?: number | null

    onClose: () => void
    onSaved?: (payload: {
        title: string
        isPublic: boolean
        thumbnailUrl?: string | null
        invitedUserId?: number | null
    }) => void
    onLiveModified?: () => void
}

