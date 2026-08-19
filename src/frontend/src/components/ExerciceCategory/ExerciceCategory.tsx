import { Typography, useTheme } from "@mui/material";
import { useTranslation } from "react-i18next";

export interface ExerciseCategoryProps {
    exerciseType?: string | null;
}

export const ExerciseCategory: React.FC<ExerciseCategoryProps> = ({ exerciseType }) => {
    const theme = useTheme();
    const { t } = useTranslation();
    
    const getColor = () => {
        if (!exerciseType) return theme.palette.action.hover;
        
        const categoryColor = theme.exerciseCategories[exerciseType];
        return categoryColor || theme.palette.action.hover;
    };

    const getLabel = () => {
        if (!exerciseType) return '';
        
        const translationKey = `administration.exercises.exerciseTypes.${exerciseType}`;
        const translated = t(translationKey);
        
        // Si no hay traducción, retorna el valor original
        return translated !== translationKey ? translated : exerciseType;
    };

    return <Typography
        variant="caption"
        sx={{
            color: '#fff',
            px: 1,
            py: 0.5,
            bgcolor: getColor(),
            borderRadius: 1,
            width: 'fit-content',
            fontWeight: 600,
        }}
    >
        {getLabel()}
    </Typography>
};